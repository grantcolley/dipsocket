using DipSocket.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DipSocket.Server;

namespace DipSocket.Client
{
    /// <summary>
    /// Send and receive <see cref="WebSocket"/> requests to a <see cref="DipSocketServer"/>
    /// </summary>
    public class DipSocketClient
    {
        private ClientWebSocket clientWebSocket;
        private Dictionary<string, Action<ServerMessage>> registeredMethods;
        private bool disposed;

        public event EventHandler<Exception> Error;
        public event EventHandler Closed;

        public string ConnectionId { get; private set; }
        public string Url { get; private set; }
        public string ClientId { get; private set; }

        public WebSocketState State { get { return clientWebSocket.State; } }

        public DipSocketClient(string url, string clientId)
        {
            Url = url;
            ClientId = clientId;

            clientWebSocket = new ClientWebSocket();
            registeredMethods = new Dictionary<string, Action<ServerMessage>>();
        }

        public async Task DisposeAsync()
        {
            if (disposed)
            {
                return;
            }

            if (clientWebSocket.State == WebSocketState.Open)
            {
                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }

            clientWebSocket.Dispose();

            disposed = true;
        }

        public void On(string methodName, Action<ServerMessage> handler)
        {
            registeredMethods.Add(methodName, handler);
        }

        public async Task StartAsync()
        {
            await clientWebSocket.ConnectAsync(new Uri($"{Url}?clientId={ClientId}"), CancellationToken.None);

            RunReceiving();
        }

        public async Task SendMessageAsync(ClientMessage message)
        {
            if (clientWebSocket.State.Equals(WebSocketState.Open))
            {
                var json = JsonConvert.SerializeObject(message);

                var bytes = Encoding.UTF8.GetBytes(json);

                await clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task StopAsync()
        {
            if (clientWebSocket.State.Equals(WebSocketState.Open))
            {
                await clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
        }

        private void OnClose(Exception exception)
        {
            var closed = Closed;
            closed.Invoke(exception);
        }

        private void RunReceiving()
        {
            Task.Run(async () =>
            {
                try
                {
                    await Receiving();
                }
                catch(Exception ex)
                {
                    OnClose(ex);
                }
            });
        }

        private async Task Receiving()
        {
            var buffer = new byte[1024 * 4];

            while (clientWebSocket.State.Equals(WebSocketState.Open))
            {
                var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType.Equals(WebSocketMessageType.Text))
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var message = JsonConvert.DeserializeObject<ServerMessage>(json);

                    if (registeredMethods.TryGetValue(message.MethodName, out Action<ServerMessage> method))
                    {
                        method.Invoke(message);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    break;
                }
            }
        }
    }
}
