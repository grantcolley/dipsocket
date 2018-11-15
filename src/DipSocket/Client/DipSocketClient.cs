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
        private Dictionary<string, Action<Message>> registeredMethods;
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
            registeredMethods = new Dictionary<string, Action<Message>>();
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

        public void On(string methodName, Action<Message> handler)
        {
            registeredMethods.Add(methodName, handler);
        }

        public async Task StartAsync()
        {
            await clientWebSocket.ConnectAsync(new Uri($"{Url}?clientId={ClientId}"), CancellationToken.None);

            RunReceiving();
        }

        public async Task SendMessageAsync(Message message)
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

        private void OnError(Exception exception)
        {
            var error = Error;
            error.Invoke(this, exception);
        }

        private void OnClose()
        {
            var closed = Closed;
            closed.Invoke(this, EventArgs.Empty);
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
                    OnError(ex);
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

                    var message = JsonConvert.DeserializeObject<Message>(json);

                    if (registeredMethods.TryGetValue(message.MethodName, out Action<Message> method))
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
