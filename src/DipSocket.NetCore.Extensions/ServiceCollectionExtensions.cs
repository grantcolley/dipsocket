using DipSocket.Server;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace DipSocket.NetCore.Extensions
{
    /// <summary>
    /// A static class for adding services to the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the generic type and each type in <see cref="Assembly.GetEntryAssembly"/> 
        /// that inherit <see cref="DipSocketServer"/> as singleton services.
        /// Add <see cref="ConnectionManager"/> and <see cref="ChannelManager"/> as transient services.
        /// </summary>
        /// <typeparam name="T">The socket server that inherits from <see cref="DipSocketServer"/>.</typeparam>
        /// <param name="servicesCollection">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDipSocket<T>(this IServiceCollection servicesCollection) where T : DipSocketServer
        {
            if (!servicesCollection.Any(s => s.ServiceType == typeof(ConnectionManager)))
            {
                servicesCollection.AddTransient<ConnectionManager>();
            }

            if (!servicesCollection.Any(s => s.ServiceType == typeof(ChannelManager)))
            {
                servicesCollection.AddTransient<ChannelManager>();
            }

            servicesCollection.AddSingleton(typeof(T));

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
