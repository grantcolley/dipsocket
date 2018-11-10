using Client.Command;
using Client.Model;
using DipSocket.Client;
using DipSocket.Messages;
using Newtonsoft.Json;
using System;
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
        private object serverInfosLock;
        private ConnectionInfo connectionInfo;
        private DipSocketClient dipSocketClient;
        
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

            serverInfosLock = new object();
            ServerInfos = new ObservableCollection<IInfo>();
            BindingOperations.EnableCollectionSynchronization(ServerInfos, serverInfosLock);
        }

        public ICommand ConnectCommand { get; set; }
        public ICommand NewChannelCommand { get; set; }
        public ICommand ClearErrorsCommand { get; set; }

        public ObservableCollection<Message> Messages { get; }
        public ObservableCollection<IInfo> ServerInfos { get; }
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

        public ConnectionInfo ConnectionInfo
        {
            get { return connectionInfo; }
            set
            {
                if (connectionInfo != value)
                {
                    connectionInfo = value;
                    OnPropertyChanged("ConnectionInfo");
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
                var clientMessage = new ClientMessage { SentBy = ConnectionInfo.Name, Data = Message, MessageType = MessageType.SendToClient };
                await dipSocketClient.SendMessageAsync(clientMessage);

                Message = string.Empty;
            }
            catch (Exception ex)
            {
                Errors.Add(new Error { Message = ex.Message, Verbose = ex.ToString() });
            }
        }

        private bool CheckConnection()
        {
            if (dipSocketClient != null
                && dipSocketClient.State.Equals(WebSocketState.Open))
            {
                return true;
            }

            Errors.Add(new Error { Message = "Connection isn't open.", Verbose = "Connection isn't open." });
            return false;
        }

        private async void OnConnect(object arg)
        {
            if (arg == null
                || string.IsNullOrWhiteSpace(arg.ToString()))
            {
                Errors.Add(new Error { Message = "A user name is required to connect to Chat", Verbose = "A user name is required to connect to Chat" });
                return;
            }

            try
            {
                dipSocketClient = new DipSocketClient(@"ws://localhost:6000/chat", arg.ToString());
                dipSocketClient.Closed += DipSocketClientClosed; ;
                dipSocketClient.Error += DipSocketClientError;

                dipSocketClient.On("OnConnected", (result) =>
                {
                    var message = (Message)result;
                    ConnectionInfo = JsonConvert.DeserializeObject<ConnectionInfo>(message.Data);
                    ConnectionMessage = $"{message.SentOn} {message.SentBy} {ConnectionInfo.Name} connected. Connection Id : {ConnectionInfo.ConnectionId}";
                    IsConnected = true;
                });

                dipSocketClient.On("OnMessageReceived", (result) =>
                {
                    lock (messagesLock)
                    {
                        Messages.Add(result);
                    }
                });

                dipSocketClient.On("OnServerInfo", (result) =>
                {
                    lock (serverInfosLock)
                    {
                        var serverInfo = JsonConvert.DeserializeObject<ServerInfo>(result.Data);
                        var allServerInfos = serverInfo.Channels.Cast<IInfo>().Union(serverInfo.Connections.Cast<IInfo>()).OrderBy(c => c.Name).ToList();

                        var removals = ServerInfos.Where(c => !allServerInfos.Any(nc => nc.Name.Equals(c)));
                        foreach (var removal in removals)
                        {
                            ServerInfos.Remove(removal);
                        }

                        var additions = allServerInfos.Where(a => !ServerInfos.Any(c => c.Name.Equals(a.Name) && !a.Name.Equals(ConnectionInfo.Name)));
                        if(additions.Any())
                        {
                            foreach (var addition in additions)
                            {
                                ServerInfos.Add(addition);
                            }
                        }
                    }
                });

                await dipSocketClient.StartAsync();

                IsConnected = true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                Errors.Add(new Error { Message = ex.Message, Verbose = ex.ToString() });
            }
        }

        private void DipSocketClientError(object sender, Exception ex)
        {
            Errors.Add(new Error { Message = ex.Message, Verbose = ex.ToString() });
        }

        private void DipSocketClientClosed(object sender, EventArgs e)
        {
            Errors.Add(new Error { Message = "Connection Closed" });
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
                var clientMessage = new ClientMessage { SentBy = ConnectionInfo.Name, Data = NewChannel, MessageType = MessageType.CreateNewChannel };

                await dipSocketClient.SendMessageAsync(clientMessage);

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