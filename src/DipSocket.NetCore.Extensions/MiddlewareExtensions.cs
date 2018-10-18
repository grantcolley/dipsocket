﻿using DipSocket;
using Microsoft.AspNetCore.Builder;
using System;

namespace DipSocket.NetCore.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseDipSocket<T>(this IApplicationBuilder builder, string route) where T : WebSocketServer, new()
        {
            var webSocketServer = Activator.CreateInstance<T>();
            webSocketServer.AddWebSocketConnections(new WebSocketConnections());
            return builder.Map(route, (applicationBuilder) => applicationBuilder.UseMiddleware<DipSocketMiddleware>(webSocketServer));
        }
    }
}
