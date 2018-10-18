using Microsoft.AspNetCore.Builder;
using System;

namespace DipSocket.NetCore.Extensions
{
    public class WebSocketServerRouteBuilder
    {
        public IApplicationBuilder MapServer<T>(IApplicationBuilder builder, string route) where T : new()
        {
            var webSocketServer = Activator.CreateInstance<T>();
            return builder.Map(route, (applicationBuilder) => applicationBuilder.UseMiddleware<DipSocketMiddleware>(webSocketServer));
        }
    }
}
