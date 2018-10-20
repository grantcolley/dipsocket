using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using DipSocket.Server;

namespace Server
{
    public class ChatServer : WebSocketServer
    {
        public async override Task OnConnectAsync(WebSocket websocket)
        {
            await base.OnConnectAsync(websocket);
            var connectionId = GetConnectionId(websocket);
            await SendMessageToAll($"{connectionId} : Connected");
        }

        public async override Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer)
        {
            var connectionId = GetConnectionId(webSocket);
            await SendMessageToAll($"{connectionId} : {Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count)}");
        }
    }
}
