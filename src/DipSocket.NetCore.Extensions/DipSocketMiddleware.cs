using DipSocket.Server;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DipSocket.NetCore.Extensions
{
    public class DipSocketMiddleware
    {
        private readonly WebSocketServer webSocketServer;

        public DipSocketMiddleware(RequestDelegate next, WebSocketServer webSocketServer)
        {
            this.webSocketServer = webSocketServer;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    var clientName = context.Request.Query["client"];
                    await webSocketServer.OnClientConnectAsync(clientName, webSocket);

                    await Receive(webSocket);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
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

        private async Task Receive(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State.Equals(WebSocketState.Open))
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType.Equals(WebSocketMessageType.Text))
                {
                    await webSocketServer.ReceiveAsync(webSocket, result, buffer);
                    continue;
                }

                if(result.MessageType.Equals(WebSocketMessageType.Close))
                {
                    await webSocketServer.OnClientDisonnectAsync(webSocket);
                    continue;
                }
            }
        }
    }
}
