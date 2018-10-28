using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using DipSocket;
using DipSocket.Server;
using Newtonsoft.Json;

namespace Server
{
    public class ChatServer : WebSocketServer
    {
        public async override Task OnConnectAsync(WebSocket websocket)
        {
            await base.OnConnectAsync(websocket);
            var connectionId = GetConnectionId(websocket);

            var message = new Message { MethodName = "OnConnected", SentBy = "Chat", Data = $"Connected : {connectionId}" };

            var json = JsonConvert.SerializeObject(message);

            await SendMessageToAll(json);
        }

        public async override Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer)
        {
            var connectionId = GetConnectionId(webSocket);
            await SendMessageToAll($"{connectionId} : {Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count)}");
        }
    }
}
