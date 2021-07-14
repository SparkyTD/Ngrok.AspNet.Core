using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Ngrok.AspNet.Core
{
    public class NgrokProcess
    {
        private static readonly Regex NgrokUrlMatch = new(@"^t=.*?msg=""started tunnel"".*?url=(.*?)$");

        private Process ngrokProcess;
        private readonly NgrokOptions options;
        private readonly bool isUsingSsl;
        private readonly string hostName;
        private readonly int portNumber;
        private readonly IServiceProvider serviceProvider;

        public NgrokProcess(NgrokOptions options, IServiceProvider serviceProvider)
        {
            this.options = options;
            this.serviceProvider = serviceProvider;

            if (!options.Enabled)
                return;

            InitializeProcess();

            var applicationUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(';');
            string selectedUrl;
            if (options.PreferHttps && applicationUrls.Any(u => u.StartsWith("https://")))
                selectedUrl = applicationUrls.First(u => u.StartsWith("https://"));
            else if (applicationUrls.Any(u => u.StartsWith("http://")))
                selectedUrl = applicationUrls.First(u => u.StartsWith("http://"));
            else
                selectedUrl = applicationUrls.First();

            var match = Regex.Match(selectedUrl, @"(https?):\/\/([\w\-.]+)(?::(\d{1,5}))?\/?");
            isUsingSsl = match.Groups[1].Value == "https";
            hostName = match.Groups[2].Value;
            string portStr = match.Groups[3].Value;

            if (!int.TryParse(portStr, out portNumber))
                portNumber = isUsingSsl ? 443 : 80;

            if (hostName == "0.0.0.0")
                hostName = "127.0.0.1";
        }

        private void InitializeProcess()
        {
            ngrokProcess = new Process();
            ngrokProcess.StartInfo.FileName = "ngrok.exe";
            ngrokProcess.StartInfo.UseShellExecute = false;
            ngrokProcess.StartInfo.CreateNoWindow = true;
            ngrokProcess.StartInfo.RedirectStandardOutput = true;
            ngrokProcess.StartInfo.RedirectStandardError = true;
            ngrokProcess.EnableRaisingEvents = true;
            ngrokProcess.OutputDataReceived += NgrokProcessOnOutputDataReceived;
            ngrokProcess.ErrorDataReceived += NgrokProcessOnErrorDataReceived;
        }

        private async void StartResetTimer()
        {
            await Task.Delay(TimeSpan.FromMinutes(110));

            ngrokProcess.Kill();
            InitializeProcess();
            StartTunnel(false);
        }

        private void NgrokProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            var urlMatch = NgrokUrlMatch.Match(e.Data);
            if (urlMatch.Success && options.TunnelHandlerType != null)
            {
                var scope = serviceProvider.CreateScope();
                var constructor = options.TunnelHandlerType.GetConstructors().First();
                var parameters = constructor.GetParameters().Select(p => serviceProvider.GetService(p.ParameterType)).ToArray();

                var instance = (INgrokTunnelUrlHandler) Activator.CreateInstance(options.TunnelHandlerType, parameters);
                instance.OnTunnelCreated(urlMatch.Groups[1].Value);

                scope.Dispose();
            }
        }

        private void NgrokProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // Console.Out.WriteLine($"[ERR] {e.Data}");
        }

        internal void StartTunnel(bool resetTimer = true)
        {
            if (!options.Enabled)
                return;
            
            ngrokProcess.StartInfo.Arguments = $"http --log stdout --log-level debug --host-header {hostName} {(isUsingSsl ? "https" : "http")}://{hostName}:{portNumber}";
            ngrokProcess.Start();
            ngrokProcess.BeginOutputReadLine();
            ngrokProcess.BeginErrorReadLine();

            ChildProcessTracker.AddProcess(ngrokProcess);

            if (resetTimer)
                StartResetTimer();
        }
    }
}