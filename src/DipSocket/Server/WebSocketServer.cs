using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DipSocket.Messages;
using System.Collections.Generic;
using System.Linq;

namespace DipSocket.Server
{
    public abstract class WebSocketServer
    {
        private WebSocketConnectionManager webSocketConnectionManager = new WebSocketConnectionManager();

        /// <summary>
        /// Creates a new instance of the WebSocketServer.
        /// </summary>
        protected WebSocketServer()
        {
            webSocketConnectionManager = new WebSocketConnectionManager();
        }

        /// <summary>
        /// Creates a new instance of the WebSocketServer.
        /// </summary>
        /// <param name="webSocketConnectionManager">An instance of the <see cref="WebSocketConnectionManager"/>. Use when injecting a singleton is preferred.</param>
        protected WebSocketServer(WebSocketConnectionManager webSocketConnectionManager)
        {
            this.webSocketConnectionManager = webSocketConnectionManager;
        }

        public abstract Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer);

        public virtual async Task OnClientConnectAsync(string clientName, WebSocket websocket)
        {
            await Task.Run(() => { webSocketConnectionManager.TryAddWebSocket(clientName, websocket); });
        }

        public virtual async Task OnClientDisonnectAsync(WebSocket webSocket)
        {
            await Task.Run(() =>
            {
                webSocket.Dispose();
                return webSocketConnectionManager.TryRemoveWebSocket(webSocket);
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

        public async Task SendMessageAsync(Connection clientConnection, ServerMessage message)
        {
            var webSocket = webSocketConnectionManager.GetWebSocket(clientConnection);
            await SendMessageAsync(webSocket, message).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(string clientName, ServerMessage message)
        {
            var webSocket = webSocketConnectionManager.GetWebSocket(clientName);
            if (webSocket != null)
            {
                await SendMessageAsync(webSocket, message).ConfigureAwait(false);
            }
        }

        public async Task SendMessageToAllAsync(ServerMessage message)
        {
            var webSockets = webSocketConnectionManager.GetWebSockets();

            // TODO: run in parallel

            foreach(var webSocket in webSockets)
            {
                await SendMessageAsync(webSocket.Value, message).ConfigureAwait(false);
            }
        }

        public void SubscribeToChannel(string channel, WebSocket webSocket)
        {
            //var clientConnection = GetClientConnection(webSocket);
            //var clientConnections = channels.GetOrAdd(channel, new List<Connection>());
            //clientConnections.Add(clientConnection);
        }

        public Connection GetClientConnection(WebSocket webSocket)
        {
            return webSocketConnectionManager.GetClientConnection(webSocket);
        }

        public ServerConnections GetServerConnections()
        {
            var connections = webSocketConnectionManager.GetWebSockets().Keys.ToList();
            var channels = connections.OfType<Channel>().ToList();

            foreach(var channel in channels)
            {
                connections.Remove(channel);
            }

            return new ServerConnections
            {
                Connections = connections,
                Channels = channels
            };
        }
    }
}