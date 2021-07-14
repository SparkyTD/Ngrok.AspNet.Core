namespace Ngrok.AspNet.Core
{
    public interface INgrokTunnelUrlHandler
    {
        void OnTunnelCreated(string tunnelUrl);
    }
}