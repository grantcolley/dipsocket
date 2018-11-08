using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using DipSocket.Messages;
using DipSocket.Server;
using Newtonsoft.Json;

namespace Server
{
    public class Chat : DipSocketServer
    {
        public Chat(ConnectionManager webSocketConnectionManager)
            : base(webSocketConnectionManager)
        {
        }

        public async override Task OnClientConnectAsync(WebSocket websocket, string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException("clientId cannot be null or empty.");
            }

            var connectionId = await base.OnClientConnectAsync(websocket);

            var messageConnected = new ServerMessage { MethodName = "OnConnected", SentBy = "Chat", Data = jsonConnected };

            await SendMessageAsync(websocket, messageConnected);

            await ChannelUpdateAsync();
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

                case MessageType.SubscribeToChannel:
                case MessageType.CreateNewChannel:
                    if(TrySubscribeToChannel(clientMessage.Data, webSocket))
                    {
                        await ChannelUpdateAsync();
                    }
                    else
                    {
                        // TODO
                    }

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
            var serverConnections = GetServerConnections();

            var serverConnectionsJson = JsonConvert.SerializeObject(serverConnections);

            var messageUpdateAll = new ServerMessage { MethodName = "OnChannelUpdate", SentBy = "Chat", Data = serverConnectionsJson };

            await SendMessageToAllAsync(messageUpdateAll);
        }
    }
}
