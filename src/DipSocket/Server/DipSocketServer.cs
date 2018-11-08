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
    /// The abstract <see cref="DipSocketServer"/> base class.
    /// </summary>
    public abstract class DipSocketServer
    {
        private ConnectionManager connectionManager;
        private ChannelManager channelManager;

        /// <summary>
        /// Creates a new instance of the <see cref="DipSocketServer"/> base class.
        /// </summary>
        protected DipSocketServer()
        {
            connectionManager = new ConnectionManager();
            channelManager = new ChannelManager();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DipSocketServer"/>.
        /// Use when injecting singleton <see cref="ConnectionManager"/> 
        /// and <see cref="ChannelManager"/> is preferred.
        /// </summary>
        /// <param name="connectionManager">An instance of the <see cref="ConnectionManager"/>.</param>
        /// <param name="channelManager">An instance of the <see cref="ChannelManager"/>.</param>
        protected DipSocketServer(ConnectionManager connectionManager, ChannelManager channelManager)
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
        /// <returns>The <see cref="Connection"/> for the <see cref="WebSocket"/>.</returns>
        public virtual async Task<Connection> OnClientDisonnectAsync(WebSocket webSocket)
        {
            if (connectionManager.TryRemoveWebSocketConnection(webSocket, out Connection connection))
            {
                connection.Channels.All(c => c.Value.Connections.TryRemove(connection.ConnectionId, out Connection removedConnection));

                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                    $"WebSocket {connection.ConnectionId} closed by {typeof(DipSocketServer).Name}", 
                    CancellationToken.None).ConfigureAwait(false);

                webSocket.Dispose();
                
                return connection;
            }

            return null;
        }

        /// <summary>
        /// Adds the <see cref="WebSocket"/> to the <see cref="ConnectionManager"/>'s web sockets dictionary.
        /// </summary>
        /// <param name="websocket">The <see cref="WebSocket"/> to add.</param>
        /// <returns>The <see cref="Connection"/> for the <see cref="WebSocket"/>.</returns>
        public virtual Task<Connection> AddWebSocketAsync(WebSocket websocket)
        {
            if (connectionManager.TryAddWebSocketConnection(websocket, out Connection connection))
            {
                return Task.FromResult<Connection>(connection);
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
            var connection = connectionManager.GetConnection(connectionId);
            if (connection != null)
            {
                await SendMessageAsync(connection.WebSocket, message).ConfigureAwait(false);
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

            var connections = connectionManager.GetConnections();

            var webSockets = from connection in connections.Values.ToArray() select SendMessageAsync(connection.WebSocket, json);

            await Task.WhenAll(webSockets.ToArray());
        }

        /// <summary>
        /// Send a message to all <see cref="WebSocket"/> clients.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task SendMessageToChannelAsync(string channelName, ServerMessage message)
        {
            var json = JsonConvert.SerializeObject(message);

            var channel = channelManager.GetChannel(channelName);

            if(channel == null)
            {
                return;
            }

            var webSockets = from connection in channel.Connections.Values.ToArray() select SendMessageAsync(connection.WebSocket, json);

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
        /// <param name="channel">The removed channel.</param>
        /// <returns>True if successful, else false.</returns>
        public bool TryRemoveChannel(string channelName, out Channel channel)
        {
            return channelManager.TryRemoveChannel(channelName, out channel);
        }
    }
}