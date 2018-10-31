using Client.Command;
using Client.Model;
using DipSocket;
using DipSocket.Client;
using DipSocket.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Windows.Data;
using System.Windows.Input;

namespace Client.ViewModel
{
    public class ChatViewModel : NotifyPropertyChangedBase
    {
        private string message;
        private string newChannel;
        private string connectionMessage;
        private bool isConnected;
        private object messagesLock;
        private object connectionsLock;
        private ClientConnection clientConnection;
        private ClientWebSocketConnection clientWebSocketConnection;
        
        public ChatViewModel()
        {
            ConnectCommand = new ViewModelCommand(OnConnect);
            NewChannelCommand = new ViewModelCommand(OnNewChannel);
            ClearErrorsCommand = new ViewModelCommand(OnClearErrors);

            Errors = new ObservableCollection<Error>();
            Errors.CollectionChanged += ErrorsCollectionChanged;

            messagesLock = new object();
            Messages = new ObservableCollection<Message>();
            BindingOperations.EnableCollectionSynchronization(Messages, messagesLock);

            connectionsLock = new object();
            Connections = new ObservableCollection<ClientConnection>();
            BindingOperations.EnableCollectionSynchronization(Connections, connectionsLock);
        }

        public ICommand ConnectCommand { get; set; }
        public ICommand NewChannelCommand { get; set; }
        public ICommand ClearErrorsCommand { get; set; }

        public ObservableCollection<Message> Messages { get; }
        public ObservableCollection<ClientConnection> Connections { get; }
        public ObservableCollection<Error> Errors { get; set; }

        public bool HasErrors
        {
            get { return Errors.Any(); }
        }

        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                    OnPropertyChanged("IsConnected");
                }
            }
        }

        public ClientConnection ClientConnection
        {
            get { return clientConnection; }
            set
            {
                if (clientConnection != value)
                {
                    clientConnection = value;
                    OnPropertyChanged("ClientConnection");
                }
            }
        }

        public string NewChannel
        {
            get { return newChannel; }
            set
            {
                if (newChannel != value)
                {
                    newChannel = value;
                    OnPropertyChanged("NewChannel");
                }
            }
        }

        public string ConnectionMessage
        {
            get { return connectionMessage; }
            set
            {
                if (connectionMessage != value)
                {
                    connectionMessage = value;
                    OnPropertyChanged("ConnectionMessage");
                }
            }
        }

        public string Message
        {
            get { return message; }
            set
            {
                if (message != value)
                {
                    message = value;
                    OnPropertyChanged("Message");
                }
            }
        }

        public async void SendMessage()
        {
            if(!CheckConnection())
            {
                return;
            }

            try
            {
                var clientMessage = new ClientMessage { SentBy = ClientConnection.Name, Data = Message, MessageType = MessageType.SendToClient };
                await clientWebSocketConnection.SendMessageAsync(clientMessage);

                Message = string.Empty;
            }
            catch (Exception ex)
            {
                Errors.Add(new Error { Message = ex.Message, Verbose = ex.ToString() });
            }
        }

        private bool CheckConnection()
        {
            if (clientWebSocketConnection != null
                && clientWebSocketConnection.State.Equals(WebSocketState.Open))
            {
                return true;
            }

            Errors.Add(new Error { Message = "Connection isn't open.", Verbose = "Connection isn't open." });
            return false;
        }

        private async void OnConnect(object arg)
        {
            if (arg != null
                || string.IsNullOrWhiteSpace(arg.ToString()))
            {
                Errors.Add(new Error { Message = "A user name is required to connect to Chat", Verbose = "A user name is required to connect to Chat" });
                return;
            }

            try
            {
                clientWebSocketConnection = new ClientWebSocketConnection(@"ws://localhost:6000/chat", arg.ToString());

                clientWebSocketConnection.On("OnConnected", (result) =>
                {
                    var message = (Message)result;
                    ClientConnection = JsonConvert.DeserializeObject<ClientConnection>(message.Data);
                    ConnectionMessage = $"{message.SentOn} {message.SentBy} {ClientConnection.Name} connected. Connection Id : {ClientConnection.ConnectionId}";
                    IsConnected = true;
                });

                clientWebSocketConnection.On("OnMessageReceived", (result) =>
                {
                    lock (messagesLock)
                    {
                        Messages.Add(result);
                    }
                });

                clientWebSocketConnection.On("OnChannelUpdate", (result) =>
                {
                    lock (connectionsLock)
                    {
                        var conns = JsonConvert.DeserializeObject<List<ClientConnection>>(result.Data);

                        var removals = Connections.Where(c => !conns.Any(nc => nc.Name.Equals(c)));
                        foreach (var removal in removals)
                        {
                            Connections.Remove(removal);
                        }

                        var additions = conns.Where(a => !Connections.Any(c => c.Name.Equals(a.Name) && !a.Name.Equals(ClientConnection.Name)));
                        if(additions.Any())
                        {
                            foreach (var addition in additions)
                            {
                                Connections.Add(addition);
                            }
                        }
                    }
                });

                await clientWebSocketConnection.StartAsync();

                IsConnected = true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                Errors.Add(new Error { Message = ex.Message, Verbose = ex.ToString() });
            }
        }

        private async void OnNewChannel(object args)
        {
            if(NewChannel == null
                || string.IsNullOrWhiteSpace(NewChannel))
            {
                Errors.Add(new Error { Message = "Specify the name of the channel you want to create.", Verbose = "Specify the name of the channel you want to create." });
                return;
            }

            if (!CheckConnection())
            {
                return;
            }

            try
            {
                var clientMessage = new ClientMessage { SentBy = ClientConnection.Name, Data = NewChannel, MessageType = MessageType.CreateNewChannel };

                await clientWebSocketConnection.SendMessageAsync(clientMessage);

                NewChannel = string.Empty;
            }
            catch (Exception ex)
            {
                Errors.Add(new Error { Message = ex.Message, Verbose = ex.ToString() });
            }
        }
        
        private void OnClearErrors(object args)
        {
            Errors.Clear();
        }

        private void ErrorsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("HasErrors");
        }
    }
}