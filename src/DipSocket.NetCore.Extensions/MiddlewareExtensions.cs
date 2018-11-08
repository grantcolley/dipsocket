using DipSocket.Server;
using Microsoft.AspNetCore.Builder;
using System;

namespace DipSocket.NetCore.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseDipSocket<T>(this IApplicationBuilder builder, string route) where T : DipSocketServer
        {
            builder.UseWebSockets();

            var connectionManager = (ConnectionManager)builder.ApplicationServices.GetService(typeof(ConnectionManager));
            var channelManager = (ChannelManager)builder.ApplicationServices.GetService(typeof(ChannelManager));
            var webSocketServer = Activator.CreateInstance(typeof(T), new object[] { connectionManager, channelManager });
            return builder.Map(route, (applicationBuilder) => applicationBuilder.UseMiddleware<DipSocketMiddleware>(webSocketServer));
        }
    }
}
