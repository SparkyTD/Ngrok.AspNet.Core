using Microsoft.Extensions.DependencyInjection;

namespace Ngrok.AspNet.Core
{
    public class NgrokOptionsBuilder
    {
        internal readonly IServiceCollection serviceCollection;

        internal NgrokOptionsBuilder(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }

        public NgrokOptions Options { get; } = new();
    }
}