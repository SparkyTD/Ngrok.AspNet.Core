using System;

namespace Ngrok.AspNet.Core
{
    public class NgrokOptions
    {
        public bool Enabled { get; set; } = true;
        public bool Inspector { get; set; } = false;
        public bool PreferHttps { get; set; } = false;
        internal Type TunnelHandlerType { get; set; }
    }
}