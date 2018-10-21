using DipSocket.Server;
using Microsoft.AspNetCore.Builder;
using System;

namespace DipSocket.NetCore.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseDipSocket<T>(this IApplicationBuilder builder, string route) where T : WebSocketServer, new()
        {
            builder.UseWebSockets();

            var webSocketServer = Activator.CreateInstance<T>();
            webSocketServer.AddWebSocketConnections(new WebSocketServerConnections());
            return builder.Map(route, (applicationBuilder) => applicationBuilder.UseMiddleware<DipSocketMiddleware>(webSocketServer));
        }
    }
}
