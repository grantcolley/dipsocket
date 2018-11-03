using DipSocket.Server;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DipSocket.NetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDipSocket(this IServiceCollection servicesCollection)
        {
            servicesCollection.AddTransient<WebSocketConnectionManager>();

            foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
            {
                if (type.GetTypeInfo().BaseType.Equals(typeof(WebSocketServer)))
                {
                    servicesCollection.AddSingleton(type);
                }
            }

            return servicesCollection;
        }
    }
}
