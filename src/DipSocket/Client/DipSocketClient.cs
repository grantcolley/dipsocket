﻿using DipSocket.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DipSocket.Server;
using System.Web;

namespace DipSocket.Client
{
    /// <summary>
    /// Send and receives <see cref="WebSocket"/> requests to a <see cref="DipSocketServer"/>
    /// </summary>
    public class DipSocketClient
    {
        private ClientWebSocket clientWebSocket;
        private Dictionary<string, Action<Message>> registeredMethods;
        private bool disposed;

        /// <summary>
        /// Raised when an exception is thrown.
        /// </summary>
        public event EventHandler<Exception> Error;

        /// <summary>
        /// Raised when the <see cref="ClientWebSocket"/> is closed.  
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Gets the connection id.
        /// </summary>
        public string ConnectionId { get; private set; }

        /// <summary>
        /// Gets the url of the <see cref="DipSocketServer"/>.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        public string ClientId { get; private set; }

        /// <summary>
        /// Gets the <see cref="ClientWebSocket"/> state.
        /// </summary>
        public WebSocketState State { get { return clientWebSocket.State; } }

        /// <summary>
        /// Creates a new instance of the <see cref="DipSocketClient"/>.
        /// </summary>
        /// <param name="url">The url of the <see cref="DipSocketServer"/>. Http and Https will be converted to ws.</param>
        /// <param name="clientId">The client side identifier.</param>
        public DipSocketClient(string url, string clientId)
        {
            if (url.ToLower().StartsWith("https"))
            {
                Url = $"ws{url.Substring(5)}";
            }
            else if (url.ToLower().StartsWith("http"))
            {
                Url = $"ws{url.Substring(4)}";
            }
            else
            {
                Url = url;
            }

            ClientId = clientId;

            clientWebSocket = new ClientWebSocket();
            registeredMethods = new Dictionary<string, Action<Message>>();
        }

        /// <summary>
        /// Close and dispose of the <see cref="ClientWebSocket"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task DisposeAsync()
        {
            if (disposed)
            {
                return;
            }

            if (clientWebSocket.State == WebSocketState.Open)
            {
                await clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }

            clientWebSocket.Dispose();

            disposed = true;
        }

        /// <summary>
        /// Register a <see cref="Action"/> to be invoked when receiving a message from the <see cref="DipSocketServer"/>.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="handler"></param>
        public void On(string methodName, Action<Message> handler)
        {
            registeredMethods.Add(methodName, handler);
        }

        /// <summary>
        /// Open a <see cref="WebSocket"/> connection with the <see cref="DipSocketServer"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task StartAsync()
        {
            await StartAsync(string.Empty);
        }

        /// <summary>
        /// Open a <see cref="WebSocket"/> connection with the <see cref="DipSocketServer"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task StartAsync(string data)
        {
            var collection = HttpUtility.ParseQueryString(string.Empty);
            collection["clientId"] = ClientId;
            collection["data"] = data;

            var uriBuilder = new UriBuilder(Url) { Query = collection.ToString() };

            await clientWebSocket.ConnectAsync(uriBuilder.Uri, CancellationToken.None);

            RunReceiving();
        }

        /// <summary>
        /// Send a mesage from to the <see cref="DipSocketServer"/> to be routed to the receipient.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> to send.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task SendMessageAsync(Message message)
        {
            if (clientWebSocket.State.Equals(WebSocketState.Open))
            {
                var json = JsonConvert.SerializeObject(message);

                var bytes = Encoding.UTF8.GetBytes(json);

                await clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        /// <summary>
        /// Close the <see cref="ClientWebSocket"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
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
            var messageBuilder = new StringBuilder();

            while (clientWebSocket.State.Equals(WebSocketState.Open))
            {
                WebSocketReceiveResult webSocketReceiveResult;

                messageBuilder.Clear();

                do
                {
                    try
                    {
                        webSocketReceiveResult = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                    catch (WebSocketException)
                    {
                        if (clientWebSocket.State.Equals(WebSocketState.Aborted))
                        {
                            break;
                        }

                        throw;
                    }

                    if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        break;
                    }
                    else if (webSocketReceiveResult.MessageType.Equals(WebSocketMessageType.Text))
                    {
                        messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count));
                    }
                }
                while (!webSocketReceiveResult.EndOfMessage);

                if(messageBuilder.Length > 0)
                {
                    var json = messageBuilder.ToString();

                    var message = JsonConvert.DeserializeObject<Message>(json);

                    if (registeredMethods.TryGetValue(message.MethodName, out Action<Message> method))
                    {
                        method.Invoke(message);
                    }
                }
            }
        }
    }
}
