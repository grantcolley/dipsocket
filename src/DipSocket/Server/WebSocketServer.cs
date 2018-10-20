using System;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("DipSocket.NetCore.Extensions")]
namespace DipSocket.Server
{
    public abstract class WebSocketServer
    {
        private WebSocketServerConnections webSocketConnections;

        public abstract Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer);

        public virtual async Task OnConnectAsync(WebSocket websocket)
        {
            await Task.Run(() => { webSocketConnections.TryAddWebSocket(websocket); });
        }

        public virtual Task OnDisonnectAsync(WebSocket webSocket)
        {
            webSocket.Dispose();
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

        public string GetConnectionId(WebSocket webSocket)
        {
            return webSocketConnections.GetConnectionId(webSocket);
        }

        internal void AddWebSocketConnections(WebSocketServerConnections webSocketConnections)
        {
            this.webSocketConnections = webSocketConnections;
        }
    }
}