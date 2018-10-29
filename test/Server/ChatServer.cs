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
        public async override Task OnConnectAsync(string clientName, WebSocket websocket)
        {
            await base.OnConnectAsync(clientName, websocket);

            var clientConnection = GetClientConnection(websocket);

            var message = new Message { MethodName = "OnConnected", SentBy = "Chat", Data = $"Connected : {clientConnection.Name}" };

            var json = JsonConvert.SerializeObject(message);

            await SendMessageToAll(json);
        }

        public async override Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer)
        {
            var clientConnection = GetClientConnection(webSocket);

            var message = new Message { MethodName = "OnMessageReceived", SentBy = clientConnection.Name, Data = Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count) };

            var json = JsonConvert.SerializeObject(message);

            await SendMessageToAll(json);
        }
    }
}
