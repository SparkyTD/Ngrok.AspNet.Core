using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ngrok.AspNet.Core
{
    public static class ServiceExtensions
    {
        public static void AddNgrok(this IServiceCollection serviceCollection, Action<NgrokOptionsBuilder> configure)
        {
            var optionsBuilder = new NgrokOptionsBuilder(serviceCollection);
            configure(optionsBuilder);
            serviceCollection.AddSingleton(optionsBuilder.Options);
            serviceCollection.AddSingleton<NgrokProcess>();
        }

        public static void AddNgrok(this IServiceCollection serviceCollection)
            => serviceCollection.AddNgrok(_ => { });

        public static NgrokOptionsBuilder TunnelHandler<T>(this NgrokOptionsBuilder optionsBuilder) where T : INgrokTunnelUrlHandler
        {
            optionsBuilder.Options.TunnelHandlerType = typeof(T);
            return optionsBuilder;
        }

        public static void UseNgrok(this IApplicationBuilder builder)
        {
            var ngrokService = builder.ApplicationServices.GetService<NgrokProcess>();
            ngrokService.StartTunnel();
        }
    }
}