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
    /// <summary>
    /// The middleware for handling requests to and responses from <see cref="DipSocketServer"/>.
    /// </summary>
    public class DipSocketMiddleware
    {
        private readonly DipSocketServer dipSocketServer;

        /// <summary>
        /// Creates an instance of the <see cref="DipSocketMiddleware"/> class.
        /// </summary>
        /// <param name="next">The <see cref="RequestDelegate"/>.</param>
        /// <param name="dipSocketServer">A specialised instance of a class inheriting <see cref="DipSocketServer"/>.</param>
        public DipSocketMiddleware(RequestDelegate next, DipSocketServer dipSocketServer)
        {
            this.dipSocketServer = dipSocketServer;
        }

        /// <summary>
        /// Receives a request to the class inheriting <see cref="DipSocketServer"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <returns>The response.</returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    var clientId = context.Request.Query["clientId"];
                    await dipSocketServer.OnClientConnectAsync(webSocket, clientId);

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
                await response.WriteAsync(JsonConvert.SerializeObject(ex)).ConfigureAwait(false);
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
                    await dipSocketServer.ReceiveAsync(webSocket, result, buffer);
                    continue;
                }

                if(result.MessageType.Equals(WebSocketMessageType.Close))
                {
                    await dipSocketServer.OnClientDisonnectAsync(webSocket);
                    continue;
                }
            }
        }
    }
}
