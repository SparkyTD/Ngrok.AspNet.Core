using System;

namespace Ngrok.AspNet.Core.Sample
{
    public class MyNgrokTunnelUrlHandler : INgrokTunnelUrlHandler
    {
        public MyNgrokTunnelUrlHandler(IServiceProvider serviceProvider)
        {
            
        }
        
        public void OnTunnelCreated(string tunnelUrl)
        {
            Console.Out.WriteLine("yay url! " + tunnelUrl);
        }
    }
}