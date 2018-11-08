using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace DipSocket.Server
{
    public sealed class Connection
    {
        internal Connection(WebSocket webSocket)
        {
            WebSocket = webSocket;
            Channels = new ConcurrentDictionary<string, Channel>();
        }

        public string Name { get; set; }
        public string ConnectionId { get; set; }
        public WebSocket WebSocket { get; }
        public ConcurrentDictionary<string, Channel> Channels { get; set; }
    }
}