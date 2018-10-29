using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("DipSocket.NetCore.Extensions")]
namespace DipSocket.Server
{
    internal class WebSocketServerConnections
    {
        private ConcurrentDictionary<ClientConnection, WebSocket> webSockets;

        internal WebSocketServerConnections()
        {
            webSockets = new ConcurrentDictionary<ClientConnection, WebSocket>();
        }

        internal WebSocket GetWebSocket(ClientConnection clientConnection)
        {
            if(clientConnection == null)
            {
                return null;
            }

            return webSockets.FirstOrDefault(ws => ws.Key.Equals(clientConnection)).Value;
        }

        internal ConcurrentDictionary<ClientConnection, WebSocket> GetWebSockets()
        {
            return webSockets;
        }

        internal ClientConnection GetClientConnection(WebSocket webSocket)
        {
            return webSockets.FirstOrDefault(ws => ws.Value.Equals(webSocket)).Key;
        }

        internal bool TryAddWebSocket(string clientName, WebSocket webSocket)
        {
            if(string.IsNullOrWhiteSpace(clientName))
            {
                return false;
            }

            var clientConnection = new ClientConnection { Name = clientName, ConnectionId = Guid.NewGuid().ToString() };
            return webSockets.TryAdd(clientConnection, webSocket);
        }

        internal async Task TryRemoveWebSocket(WebSocket webSocket)
        {
            var clientConnection = GetClientConnection(webSocket);

            WebSocket socket;
            if (webSockets.TryRemove(clientConnection, out socket))
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocketController", CancellationToken.None);
            }
        }
    }
}
