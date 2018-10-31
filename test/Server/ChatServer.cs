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
                var jsonConnected = JsonConvert.SerializeObject(clientConnection);

                var messageConnected = new ServerMessage { MethodName = "OnConnected", SentBy = "Chat", Data = jsonConnected };

                await SendMessageAsync(websocket, messageConnected);

                await ChannelUpdateAsync();
            }
        }

        public async override Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer)
        {
            var json = Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count);

            var clientMessage = JsonConvert.DeserializeObject<ClientMessage>(json);

            switch (clientMessage.MessageType)
            {
                case MessageType.SendToAll:
                    var messageAll = new ServerMessage { MethodName = "OnMessageReceived", SentBy = clientMessage.SentBy, Data = clientMessage.Data };
                    await SendMessageToAllAsync(messageAll);
                    break;

                case MessageType.SendToClient:
                    var messageClient = new ServerMessage { MethodName = "OnMessageReceived", SentBy = clientMessage.SentBy, Data = clientMessage.Data };
                    await SendMessageAsync(clientMessage.SendTo, messageClient);
                    break;

                case MessageType.CreateNewChannel:
                    // TODO

                    await ChannelUpdateAsync();
                    break;

                case MessageType.SubscribeToChannel:
                    // TODO

                    break;
                case MessageType.SendToChannel:
                    // TODO
                    break;

                case MessageType.UnsubscribeFromChannel:
                    // TODO

                    break;

                default:
                    throw new NotImplementedException($"{clientMessage.MessageType}");
            }
        }

        private async Task ChannelUpdateAsync()
        {
            var webSockets = GetClientConnections();

            var jsonUpdateAll = JsonConvert.SerializeObject(webSockets);

            var messageUpdateAll = new ServerMessage { MethodName = "OnChannelUpdate", SentBy = "Chat", Data = jsonUpdateAll };

            await SendMessageToAllAsync(messageUpdateAll);
        }
    }
}
