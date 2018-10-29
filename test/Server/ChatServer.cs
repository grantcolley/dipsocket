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
        public async override Task OnClientConnectAsync(string clientName, WebSocket websocket)
        {
            await base.OnClientConnectAsync(clientName, websocket);

            var clientConnection = GetClientConnection(websocket);

            var message = new Message { MethodName = "OnConnected", SentBy = "Chat", Data = $"Connected : {clientConnection.Name}" };

            var json = JsonConvert.SerializeObject(message);

            await SendMessageToAll(json);
        }

        public async override Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer)
        {
            var clientConnection = GetClientConnection(webSocket);

            // TODO: var message = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count));

            var message = new Message { MethodName = "OnMessageReceived", SentBy = clientConnection.Name, Data = Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count) };

            var json = JsonConvert.SerializeObject(message);

            await SendMessageToAll(json);
        }
    }
}
