using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DipSocket.Client
{
    public class ClientWebSocketConnection
    {
        private ClientWebSocket clientWebSocket;
        private Dictionary<string, Action<object>> registeredMethods;
        private bool disposed;

        public event Func<Exception, Task> Closed;

        public string ConnectionId { get; private set; }
        public string Url { get; private set; }

        public ClientWebSocketConnection(string url)
        {
            Url = url;

            clientWebSocket = new ClientWebSocket();
            registeredMethods = new Dictionary<string, Action<object>>();
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

        public void On(string methodName, Action<object> handler)
        {
            registeredMethods.Add(methodName, handler);
        }

        public async Task StartAsync()
        {
            await clientWebSocket.ConnectAsync(new Uri(Url), CancellationToken.None);

            RunReceiving();
        }

        public async Task SendMessageAsync(string message)
        {
            if (clientWebSocket.State.Equals(WebSocketState.Open))
            {
                var bytes = Encoding.UTF8.GetBytes(message);

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
            Task.Run(() =>
            {
                try
                {
                    Receiving();
                }
                catch(Exception ex)
                {
                    // todo better exception handling
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

                    if (registeredMethods.TryGetValue(message.MethodName, out Action<object> method))
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
