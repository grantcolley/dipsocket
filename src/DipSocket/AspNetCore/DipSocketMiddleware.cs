using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace DipSocket.AspNetCore
{
    public class DipSocketMiddleware
    {
        public DipSocketMiddleware(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await Echo(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
            }
            catch (Exception ex)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await response.WriteAsync(JsonConvert.SerializeObject(ex));
            }
        }
    }
}
