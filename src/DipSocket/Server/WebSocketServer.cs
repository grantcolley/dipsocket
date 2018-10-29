using System;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DipSocket.Server
{
    public abstract class WebSocketServer
    {
        private WebSocketServerConnections webSocketConnections = new WebSocketServerConnections();

        public abstract Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer);

        public virtual async Task OnClientConnectAsync(string clientName, WebSocket websocket)
        {
            await Task.Run(() => { webSocketConnections.TryAddWebSocket(clientName, websocket); });
        }

        public virtual Task OnClientDisonnectAsync(WebSocket webSocket)
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

        public async Task SendMessageAsync(ClientConnection clientConnection, string message)
        {
            var webSocket = webSocketConnections.GetWebSocket(clientConnection);
            await SendMessageAsync(webSocket, message).ConfigureAwait(false);
        }

        public async Task SendMessageToAll(string message)
        {
            var webSockets = webSocketConnections.GetWebSockets();
            foreach(var webSocket in webSockets)
            {
                await SendMessageAsync(webSocket.Value, message).ConfigureAwait(false);
            }
        }

        public ClientConnection GetClientConnection(WebSocket webSocket)
        {
            return webSocketConnections.GetClientConnection(webSocket);
        }
    }
}