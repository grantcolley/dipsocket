using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DipSocket.Messages;

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

        public virtual async Task OnClientDisonnectAsync(WebSocket webSocket)
        {
            await Task.Run(() =>
            {
                webSocket.Dispose();
                return webSocketConnections.TryRemoveWebSocket(webSocket);
            });
        }

        public async Task SendMessageAsync(WebSocket webSocket, ServerMessage message)
        {
            if(!webSocket.State.Equals(WebSocketState.Open))
            {
                return;
            }

            var json = JsonConvert.SerializeObject(message);

            await webSocket.SendAsync(
                new ArraySegment<byte>(Encoding.ASCII.GetBytes(json), 0, json.Length), 
                WebSocketMessageType.Text, true, CancellationToken.None)
                .ConfigureAwait(false); 
        }

        public async Task SendMessageAsync(ClientConnection clientConnection, ServerMessage message)
        {
            var webSocket = webSocketConnections.GetWebSocket(clientConnection);
            await SendMessageAsync(webSocket, message).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(string clientName, ServerMessage message)
        {
            var webSocket = webSocketConnections.GetWebSocket(clientName);
            if (webSocket != null)
            {
                await SendMessageAsync(webSocket, message).ConfigureAwait(false);
            }
        }

        public async Task SendMessageToAllAsync(ServerMessage message)
        {
            var webSockets = webSocketConnections.GetWebSockets();

            // TODO: run in parallel

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