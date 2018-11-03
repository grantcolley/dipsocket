using DipSocket.Server;
using Microsoft.AspNetCore.Builder;
using System;

namespace DipSocket.NetCore.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseDipSocket<T>(this IApplicationBuilder builder, string route) where T : WebSocketServer
        {
            builder.UseWebSockets();

            var webSocketConnectionManager = (WebSocketConnectionManager)builder.ApplicationServices.GetService(typeof(WebSocketConnectionManager));
            var webSocketServer = Activator.CreateInstance(typeof(T), webSocketConnectionManager);
            return builder.Map(route, (applicationBuilder) => applicationBuilder.UseMiddleware<DipSocketMiddleware>(webSocketServer));
        }
    }
}
