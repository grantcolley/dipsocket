using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DipSocket.Server
{
    public sealed class WebSocketConnectionManager
    {
        private ConcurrentDictionary<Connection, WebSocket> connections;

        public WebSocketConnectionManager()
        {
            connections = new ConcurrentDictionary<Connection, WebSocket>();
        }

        internal WebSocket GetWebSocket(string clientName)
        {
            var clientConnection = connections.Keys.FirstOrDefault(c => c.Name.Equals(clientName));
            return GetWebSocket(clientConnection);
        }

        internal WebSocket GetWebSocket(Connection clientConnection)
        {
            if(clientConnection == null)
            {
                return null;
            }

            if (connections.TryGetValue(clientConnection, out WebSocket webSocket))
            {
                return webSocket;
            }

            return null;
        }

        internal ConcurrentDictionary<Connection, WebSocket> GetWebSockets()
        {
            return connections;
        }

        internal Connection GetClientConnection(WebSocket webSocket)
        {
            if(connections.Values.Contains(webSocket))
            {
                return connections.FirstOrDefault(ws => ws.Value.Equals(webSocket)).Key;
            }

            return null;
        }

        internal bool TryAddWebSocket(string clientName, WebSocket webSocket)
        {
            if(string.IsNullOrWhiteSpace(clientName))
            {
                return false;
            }

            var clientConnection = new Connection { Name = clientName, ConnectionId = Guid.NewGuid().ToString() };
            return connections.TryAdd(clientConnection, webSocket);
        }

        internal async Task TryRemoveWebSocket(WebSocket webSocket)
        {
            var clientConnection = GetClientConnection(webSocket);

            WebSocket socket;
            if (connections.TryRemove(clientConnection, out socket))
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocketController", CancellationToken.None);
            }
        }
    }
}
