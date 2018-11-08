using DipSocket.Server;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DipSocket.NetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDipSocket(this IServiceCollection servicesCollection)
        {
            servicesCollection.AddTransient<ConnectionManager>();
            servicesCollection.AddTransient<ChannelManager>();

            foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
            {
                if (type.GetTypeInfo().BaseType.Equals(typeof(DipSocketServer)))
                {
                    servicesCollection.AddSingleton(type);
                }
            }

            return servicesCollection;
        }
    }
}
