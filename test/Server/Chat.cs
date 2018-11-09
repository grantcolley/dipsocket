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
        public Chat(ConnectionManager webSocketConnectionManager, ChannelManager channelManager)
            : base(webSocketConnectionManager, channelManager)
        {
        }

        public async override Task OnClientConnectAsync(WebSocket websocket, string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException("clientId cannot be null or empty.");
            }

            var connection = await base.AddWebSocketAsync(websocket).ConfigureAwait(false);

            if (connection != null)
            {
                connection.Name = clientId;

                var connectionInfo = connection.GetConnectionInfo();

                var json = JsonConvert.SerializeObject(connectionInfo);

                var serverMessage = new ServerMessage { MethodName = "OnConnected", SentBy = "Chat", Data = json };

                await SendMessageAsync(websocket, serverMessage).ConfigureAwait(false);

                await ChannelUpdateAsync().ConfigureAwait(false); ;
            }
            else
            {
                var serverMessage = new ServerMessage { MethodName = "OnConnected", SentBy = "Chat", Data = $"{clientId} failed to connect." };

                await SendMessageAsync(websocket, serverMessage).ConfigureAwait(false); ;
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
                    await SendMessageToAllAsync(messageAll).ConfigureAwait(false);
                    break;

                case MessageType.SendToClient:
                    var messageClient = new ServerMessage { MethodName = "OnMessageReceived", SentBy = clientMessage.SentBy, Data = clientMessage.Data };
                    await SendMessageAsync(clientMessage.SendTo, messageClient).ConfigureAwait(false);
                    break;

                case MessageType.SubscribeToChannel:
                case MessageType.CreateNewChannel:
                    var channel = SubscribeToChannel(clientMessage.Data, webSocket);
                    if (channel != null)
                    {
                        var channelInfo = channel.GetChannelInfo();
                        var channelInfoJson = JsonConvert.SerializeObject(channelInfo);
                        var subscribeMessage = new ServerMessage { MethodName = "OnMessageReceived", SentBy = channel.Name, Data = channelInfoJson };
                        await SendMessageToChannelAsync(channel, subscribeMessage).ConfigureAwait(false);
                        await ChannelUpdateAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        var subscribeMessage = new ServerMessage { MethodName = "OnMessageReceived", SentBy = "Chat", Data = $"Failed to subscribe to channel {clientMessage.Data}." };
                        await SendMessageAsync(webSocket, subscribeMessage).ConfigureAwait(false);
                    }

                    break;

                case MessageType.SendToChannel:
                    var channelMessage = new ServerMessage { MethodName = "OnMessageReceived", SentBy = clientMessage.SentBy, Data = clientMessage.Data };
                    await SendMessageToChannelAsync(clientMessage.SendTo, channelMessage).ConfigureAwait(false);
                    break;

                case MessageType.UnsubscribeFromChannel:
                    var unsubscribedChannel = UnsubscribeFromChannel(clientMessage.Data, webSocket);
                    await ChannelUpdateAsync().ConfigureAwait(false);
                    break;

                default:
                    throw new NotImplementedException($"{clientMessage.MessageType}");
            }
        }

        private async Task ChannelUpdateAsync()
        {
            var serverInfo = GetServerInfo();

            var json = JsonConvert.SerializeObject(serverInfo);

            var messageUpdateAll = new ServerMessage { MethodName = "OnChannelUpdate", SentBy = "Chat", Data = json };

            await SendMessageToAllAsync(messageUpdateAll);
        }
    }
}
