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

                Console.WriteLine(json);

                var message = new Message { MethodName = "OnConnected", Sender = "Chat", Data = json };

                await SendMessageAsync(websocket, message).ConfigureAwait(false);

                await ChannelUpdateAsync().ConfigureAwait(false);
            }
            else
            {
                var message = new Message { MethodName = "OnConnected", Sender = "Chat", Data = $"{clientId} failed to connect." };

                await SendMessageAsync(websocket, message).ConfigureAwait(false);
            }
        }

        public async override Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer)
        {
            try
            {
                var json = Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count);

                Console.WriteLine(json);

                var message = JsonConvert.DeserializeObject<Message>(json);

                switch (message.MessageType)
                {
                    case MessageType.SendToAll:
                        message.MethodName = "OnMessageReceived";
                        await SendMessageToAllAsync(message).ConfigureAwait(false);
                        break;

                    case MessageType.SendToClient:
                        message.MethodName = "OnMessageReceived";                        
                        await SendMessageAsync(message.RecipientConnectionId, message).ConfigureAwait(false);
                        break;

                    case MessageType.SubscribeToChannel:
                        var channel = SubscribeToChannel(message.Data, webSocket);
                        if (channel != null)
                        {
                            await ChannelUpdateAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            var errorMessage = new Message { MethodName = "OnMessageError", SenderConnectionId = "Chat", Data = $"Failed to subscribe to channel {message.Data}." };
                            await SendErrorMessage(webSocket, errorMessage).ConfigureAwait(false);
                        }

                        break;

                    case MessageType.SendToChannel:
                        message.MethodName = "OnMessageReceived";
                        await SendMessageToChannelAsync(message.RecipientConnectionId, message).ConfigureAwait(false);
                        break;

                    case MessageType.UnsubscribeFromChannel:
                        var unsubscribedChannel = UnsubscribeFromChannel(message.Data, webSocket);
                        await ChannelUpdateAsync().ConfigureAwait(false);
                        break;

                    case MessageType.Disconnect:
                        // TODO
                        break;
                }
            }
            catch(Exception ex)
            {
                var errorMessage = new Message { MethodName = "OnMessageError", SenderConnectionId = "Chat", Data = $"Chat Error : {ex.Message}" };
                await SendErrorMessage(webSocket, errorMessage).ConfigureAwait(false);
            }
        }

        private async Task SendErrorMessage(WebSocket webSocket, Message message)
        {
            try
            {
                await SendMessageAsync(webSocket, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // TODO : server side logging... 
            }
        }

        private async Task ChannelUpdateAsync()
        {
            var serverInfo = GetServerInfo();

            var json = JsonConvert.SerializeObject(serverInfo);

            var messageUpdateAll = new Message { MethodName = "OnServerInfo", SenderConnectionId = "Chat", Data = json };

            await SendMessageToAllAsync(messageUpdateAll);
        }
    }
}
