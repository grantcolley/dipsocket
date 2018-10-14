using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DipSocket.Server
{
    internal class WebSocketController
    {
        private ConcurrentDictionary<string, WebSocket> webSockets;

        internal WebSocketController()
        {
            webSockets = new ConcurrentDictionary<string, WebSocket>();
        }

        internal WebSocket GetWebSocket(string connectionId)
        {
            if(string.IsNullOrWhiteSpace(connectionId))
            {
                return null;
            }

            return webSockets.FirstOrDefault(ws => ws.Key.Equals(connectionId)).Value;
        }

        internal ConcurrentDictionary<string, WebSocket> GetWebSockets()
        {
            return webSockets;
        }

        internal string GetConnectionId(WebSocket webSocket)
        {
            return webSockets.FirstOrDefault(ws => ws.Value.Equals(webSocket)).Key;
        }

        internal bool TryAddWebSocket(WebSocket webSocket)
        {
            var connectionId = NewConnectionId();
            return webSockets.TryAdd(connectionId, webSocket);
        }

        internal async Task TryRemoveWebSocket(WebSocket webSocket)
        {
            var connectionId = GetConnectionId(webSocket);

            WebSocket socket;
            if (webSockets.TryRemove(connectionId, out socket))
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocketController", CancellationToken.None);
            }
        }

        private string NewConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
