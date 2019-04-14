using DipSocket.Messages;
using DipSocket.Server;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
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
                    var data = context.Request.Query["data"];

                    await dipSocketServer.OnClientConnectAsync(webSocket, clientId, data);

                    await Receive(webSocket);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            catch (WebSocketException wsex) when (wsex.WebSocketErrorCode.Equals(WebSocketError.ConnectionClosedPrematurely))
            {
                // The remote party closed the WebSocket connection
                // without completing the close handshake.
            }
            catch (Exception ex)
            {
                var response = context.Response;
                response.Clear();
                response.ContentType = "application/json";
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await response.WriteAsync(JsonConvert.SerializeObject(ex)).ConfigureAwait(false);
            }
        }

        private async Task Receive(WebSocket webSocket)
        {
            try
            {
                var buffer = new byte[1024 * 4];
                var messageBuilder = new StringBuilder();

                while (webSocket.State.Equals(WebSocketState.Open))
                {
                    WebSocketReceiveResult webSocketReceiveResult;

                    messageBuilder.Clear();

                    do
                    {
                        webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (webSocketReceiveResult.MessageType.Equals(WebSocketMessageType.Close))
                        {
                            await dipSocketServer.OnClientDisonnectAsync(webSocket);
                            continue;
                        }

                        if (webSocketReceiveResult.MessageType.Equals(WebSocketMessageType.Text))
                        {
                            messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count));
                            continue;
                        }
                    }
                    while (!webSocketReceiveResult.EndOfMessage);

                    if (messageBuilder.Length > 0)
                    {
                        var json = messageBuilder.ToString();

                        var message = JsonConvert.DeserializeObject<Message>(json);

                        await dipSocketServer.ReceiveAsync(webSocket, message);
                    }
                }
            }
            finally
            {
                webSocket?.Dispose();
            }
        }
    }
}
