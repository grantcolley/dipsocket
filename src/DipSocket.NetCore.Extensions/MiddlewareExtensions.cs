using DipSocket.Server;
using Microsoft.AspNetCore.Builder;

namespace DipSocket.NetCore.Extensions
{
    /// <summary>
    /// Static class containing the extension method for adding the <see cref="DipSocketMiddleware"/> to <see cref="IApplicationBuilder"/>. 
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Extension method for adding the <see cref="DipSocketMiddleware"/> to <see cref="IApplicationBuilder"/>. 
        /// </summary>
        /// <typeparam name="T">A type that inherits <see cref="DipSocketServer"/>.</typeparam>
        /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="route">The route to map to the <see cref="DipSocketMiddleware"/>.</param>
        /// <returns>An instance of <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseDipSocket<T>(this IApplicationBuilder builder, string route) where T : DipSocketServer
        {
            builder.UseWebSockets();
            var webSocketServer = (T)builder.ApplicationServices.GetService(typeof(T));
            return builder.Map(route, (applicationBuilder) => applicationBuilder.UseMiddleware<DipSocketMiddleware>(webSocketServer));
        }
    }
}
