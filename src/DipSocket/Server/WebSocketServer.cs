using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DipSocket.Messages;
using System.Linq;

namespace DipSocket.Server
{
    /// <summary>
    /// The abstract <see cref="WebSocketServer"/> base class.
    /// </summary>
    public abstract class WebSocketServer
    {
        private ConnectionManager connectionManager;
        private ChannelManager channelManager;

        /// <summary>
        /// Creates a new instance of the <see cref="WebSocketServer"/> base class.
        /// </summary>
        protected WebSocketServer()
        {
            connectionManager = new ConnectionManager();
            channelManager = new ChannelManager();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WebSocketServer"/>.
        /// Use when injecting singleton <see cref="ConnectionManager"/> 
        /// and <see cref="ChannelManager"/> is preferred.
        /// </summary>
        /// <param name="connectionManager">An instance of the <see cref="ConnectionManager"/>.</param>
        /// <param name="channelManager">An instance of the <see cref="ChannelManager"/>.</param>
        protected WebSocketServer(ConnectionManager connectionManager, ChannelManager channelManager)
        {
            this.connectionManager = connectionManager;
            this.channelManager = channelManager;
        }

        /// <summary>
        /// Receive a message from a <see cref="WebSocket"/>.
        /// </summary>
        /// <param name="webSocket">The <see cref="WebSocket"/>.</param>
        /// <param name="webSocketReceiveResult">The <see cref="WebSocketReceiveResult"/>.</param>
        /// <param name="buffer">The message.</param>
        /// <returns>A <see="Task"/>.</returns>
        public abstract Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer);

        /// <summary>
        /// Handle a <see cref="WebSocket"/> client connection.
        /// </summary>
        /// <param name="websocket">The <see cref="WebSocket"/>.</param>
        /// <param name="data">The data provided by the <see cref="WebSocket"/>.</param>
        /// <returns>A <see="Task"/>.</returns>
        public abstract Task OnClientConnectAsync(WebSocket websocket, string data);

        /// <summary>
        /// Removes the <see cref="WebSocket"/> <see cref="ConnectionManager"/>'s web sockets dictionary, 
        /// calls the web sockets CloseAsync method and then disposes it.
        /// </summary>
        /// <param name="webSocket">The <see cref="WebSocket"/> to remove.</param>
        /// <returns>The connectionId for the <see cref="WebSocket"/>.</returns>
        public virtual async Task<string> OnClientDisonnectAsync(WebSocket webSocket)
        {
            if (connectionManager.TryRemoveWebSocket(webSocket, out string connectionId))
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                    $"WebSocket {connectionId} closed by {typeof(WebSocketServer).Name}", 
                    CancellationToken.None).ConfigureAwait(false);

                webSocket.Dispose();

                return connectionId;
            }

            return null;
        }

        /// <summary>
        /// Adds the <see cref="WebSocket"/> to the <see cref="ConnectionManager"/>'s web sockets dictionary.
        /// </summary>
        /// <param name="websocket">The <see cref="WebSocket"/> to add.</param>
        /// <returns>The connectionId for the <see cref="WebSocket"/>.</returns>
        public virtual Task<string> AddWebSocketAsync(WebSocket websocket)
        {
            if (connectionManager.TryAddWebSocket(websocket, out string connectionId))
            {
                return Task.FromResult<string>(connectionId);
            }

            return null;
        }

        /// <summary>
        /// Send a message to a <see cref="WebSocket"/> client.
        /// </summary>
        /// <param name="connectionId">The connection Id of the <see cref="WebSocket"/> client.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task SendMessageAsync(string connectionId, ServerMessage message)
        {
            var webSocket = connectionManager.GetWebSocket(connectionId);
            if (webSocket != null)
            {
                await SendMessageAsync(webSocket, message).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Send a message to all <see cref="WebSocket"/> clients.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task SendMessageToAllAsync(ServerMessage message)
        {
            var json = JsonConvert.SerializeObject(message);

            var webSocketClients = connectionManager.GetWebSocketsConnections();

            var webSockets = from websocket in webSocketClients.Values.ToArray() select SendMessageAsync(websocket, json);

            await Task.WhenAll(webSockets.ToArray());
        }

        /// <summary>
        /// Send a message to a <see cref="WebSocket"/> client.
        /// </summary>
        /// <param name="webSocket">The <see cref="WebSocket"/> to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task SendMessageAsync(WebSocket webSocket, ServerMessage message)
        {
            var json = JsonConvert.SerializeObject(message);

            await SendMessageAsync(webSocket, json); 
        }

        /// <summary>
        /// Send a message to a <see cref="WebSocket"/> client.
        /// </summary>
        /// <param name="webSocket">The <see cref="WebSocket"/> to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task SendMessageAsync(WebSocket webSocket, string message)
        {
            if (!webSocket.State.Equals(WebSocketState.Open))
            {
                return;
            }

            await webSocket.SendAsync(
                new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length),
                WebSocketMessageType.Text, true, CancellationToken.None)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribe to a <see cref="Channel"/>. If the 
        /// <see cref="Channel"/> doesn't exist then create one.
        /// </summary>
        /// <param name="channelName">The channel to subscribe to.</param>
        /// <param name="connection">The connection subscribing to the channel.</param>
        /// <returns>The <see cref="Channel"/>.</returns>
        public Channel SubscribeToChannel(string channelName, Connection connection)
        {
            return channelManager.SubscribeToChannel(channelName, connection);
        }

        /// <summary>
        /// Unsubscribe from a <see cref="Channel"/>. If the 
        /// <see cref="Channel"/> no longer has any <see cref="Connection"/>'s 
        /// then remove the <see cref="Channel"/>.
        /// </summary>
        /// <param name="channelName">The channel to unsubscribe from.</param>
        /// <param name="connection">The connection unsubscribing from the channel.</param>
        /// <returns>The <see cref="Channel"/>.</returns>
        public Channel UnsubscribeFromChannel(string channelName, Connection connection)
        {
            return channelManager.UnsubscribeFromChannel(channelName, connection);
        }

        /// <summary>
        /// Get a <see cref="Channel"/>.
        /// </summary>
        /// <param name="channelName">The <see cref="Channel"/> to get.</param>
        /// <returns>The <see cref="Channel"/>.</returns>
        public Channel GetChannel(string channelName)
        {
            return channelManager.GetChannel(channelName);
        }

        /// <summary>
        /// Remove a <see cref="Channel"/>.
        /// </summary>
        /// <param name="channelName">The <see cref="Channel"/> to remove.</param>
        /// <returns>True if successful, else false.</returns>
        public bool TryRemoveChannel(string channelName)
        {
            return channelManager.TryRemoveChannel(channelName);
        }
    }
}