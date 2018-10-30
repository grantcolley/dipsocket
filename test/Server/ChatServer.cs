using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using DipSocket.Messages;
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

            if (clientConnection != null)
            {
                var json = JsonConvert.SerializeObject(clientConnection);

                var message = new ServerMessage { MethodName = "OnConnected", SentBy = "Chat", Data = json };

                await SendMessageAsync(websocket, message);
            }
        }

        public async override Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer)
        {
            var json = Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count);

            var clientMessage = JsonConvert.DeserializeObject<ClientMessage>(json);

            switch (clientMessage.MessageType)
            {
                case MessageType.All:
                    var messageAll = new ServerMessage { MethodName = "OnMessageReceived", SentBy = clientMessage.SentBy, Data = clientMessage.Data };
                    await SendMessageToAllAsync(messageAll);
                    break;

                case MessageType.Client:
                    var messageClient = new ServerMessage { MethodName = "OnMessageReceived", SentBy = clientMessage.SentBy, Data = clientMessage.Data };
                    await SendMessageAsync(clientMessage.SendTo, messageClient);
                    break;

                case MessageType.Group:
                    // TODO
                    break;

                case MessageType.NewChannel:
                    // TODO
                    break;

                default:
                    throw new NotImplementedException($"{clientMessage.MessageType}");
            }
        }
    }
}
