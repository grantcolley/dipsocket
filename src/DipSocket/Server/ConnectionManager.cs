using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace DipSocket.Server
{
    /// <summary>
    /// The <see cref="ConnectionManager"/> class stores and manages access to a 
    /// <see cref="ConcurrentDictionary{string, WebSocket}"/> of client <see cref="WebSocket"/>'s.
    /// </summary>
    public sealed class ConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> webSockets;
        private object connectionLock = new object();

        /// <summary>
        /// Creates a new instance of the <see cref="ConnectionManager"/> class.
        /// </summary>
        public ConnectionManager()
        {
            webSockets = new ConcurrentDictionary<string, WebSocket>();
        }

        internal ConcurrentDictionary<string, WebSocket> GetWebSocketsConnections()
        {
            return webSockets;
        }

        internal WebSocket GetWebSocket(string connectionId)
        {
            if (webSockets.TryGetValue(connectionId, out WebSocket webSocket))
            {
                return webSocket;
            }

            return null;
        }

        internal string GetConnectionId(WebSocket webSocket)
        {
            foreach(var kvp in webSockets)
            {
                if(kvp.Value == webSocket)
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        internal bool TryAddWebSocket(WebSocket webSocket, out string connectionId)
        {
            connectionId = Guid.NewGuid().ToString();
            return webSockets.TryAdd(connectionId, webSocket);
        }

        internal bool TryRemoveWebSocket(WebSocket webSocket, out string connectionId)
        {
            connectionId = GetConnectionId(webSocket);
            return webSockets.TryRemove(connectionId, out WebSocket socket);
        }
    }
}