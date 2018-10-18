using System;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("DipSocket.NetCore.Extensions")]
namespace DipSocket
{
    public abstract class WebSocketServer
    {
        private WebSocketConnections webSocketConnections;

        public abstract Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer);

        public virtual async Task<bool> OnConnectAsync(WebSocket websocket)
        {
            return webSocketConnections.TryAddWebSocket(websocket);
        }

        public virtual Task OnDisonnectAsync(WebSocket webSocket)
        {
            return webSocketConnections.TryRemoveWebSocket(webSocket);
        }

        public async Task SendMessageAsync(WebSocket webSocket, string message)
        {
            if(!webSocket.State.Equals(WebSocketState.Open))
            {
                return;
            }

            await webSocket.SendAsync(
                new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length), 
                WebSocketMessageType.Text, true, CancellationToken.None)
                .ConfigureAwait(false); 
        }

        public async Task SendMessageAsync(string connectionId, string message)
        {
            var webSocket = webSocketConnections.GetWebSocket(connectionId);
            await SendMessageAsync(webSocket, message).ConfigureAwait(false);
        }

        public async Task SendMessageToAll(string message)
        {
            foreach(var webSocket in webSocketConnections.GetWebSockets())
            {
                await SendMessageAsync(webSocket.Value, message).ConfigureAwait(false);
            }
        }

        internal void AddWebSocketConnections(WebSocketConnections webSocketConnections)
        {
            this.webSocketConnections = webSocketConnections;
        }
    }
}