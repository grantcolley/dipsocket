using DipSocket.Server;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DipSocket.NetCore.Extensions
{
    /// <summary>
    /// A static class for adding services to the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register each type in <see cref="Assembly.GetEntryAssembly"/> 
        /// that inherit <see cref="DipSocketServer"/> as a singleton service.
        /// Add <see cref="ConnectionManager"/> and <see cref="ChannelManager"/> as transient services.
        /// </summary>
        /// <param name="servicesCollection">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
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
