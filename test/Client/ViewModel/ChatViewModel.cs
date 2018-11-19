using Client.Command;
using Client.Model;
using DipSocket.Client;
using DipSocket.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Client.ViewModel
{
    public class ChatViewModel : NotifyPropertyChangedBase
    {
        private string message;
        private string addInfoName;
        private object serverInfosLock;
        private object errorsLock;
        private ConnectionInfo user;
        private DipSocketClient dipSocketClient;
        private InfoDecorator selectedInfo;
        private Dispatcher dispatcher;

        public ChatViewModel()
        {
            ConnectCommand = new ViewModelCommand(OnConnect);
            AddInfoCommand = new ViewModelCommand(OnAddInfo);
            SendMessageCommand = new ViewModelCommand(OnSendMessage);
            RemoveCommand = new ViewModelCommand(OnRemoveItem);
            ClearErrorsCommand = new ViewModelCommand(OnClearErrors);

            serverInfosLock = new object();

            errorsLock = new object();
            Errors = new ObservableCollection<Error>();
            Errors.CollectionChanged += ErrorsCollectionChanged;
            BindingOperations.EnableCollectionSynchronization(Errors, errorsLock);

            ServerInfos = new ObservableCollection<IInfo>();
            UserInfos = new ObservableCollection<IInfo>();

            dispatcher = Application.Current.Dispatcher;
        }

        public ICommand ConnectCommand { get; set; }
        public ICommand AddInfoCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand ClearErrorsCommand { get; set; }

        public ObservableCollection<IInfo> ServerInfos { get; }
        public ObservableCollection<IInfo> UserInfos { get; }
        public ObservableCollection<Error> Errors { get; set; }

        public bool HasErrors
        {
            get { return Errors.Any(); }
        }

        public ConnectionInfo User
        {
            get { return user; }
            set
            {
                if (user != value)
                {
                    user = value;
                    OnPropertyChanged("ConnectionInfo");
                }
            }
        }

        public InfoDecorator SelectedInfo
        {
            get { return selectedInfo; }
            set
            {
                if(selectedInfo != value)
                {
                    selectedInfo = value;
                    OnPropertyChanged("SelectedInfo");
                }
            }
        }

        public string AddInfoName
        {
            get { return addInfoName; }
            set
            {
                if (addInfoName != value)
                {
                    addInfoName = value;
                    OnPropertyChanged("AddInfoName");
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

        private async void OnConnect(object arg)
        {
            try
            {
                dipSocketClient = new DipSocketClient(@"ws://localhost:6000/chat", arg.ToString());
                dipSocketClient.Closed += DipSocketClientClosed; ;
                dipSocketClient.Error += DipSocketClientError;

                dipSocketClient.On("OnConnected", (result) =>
                {
                    User = JsonConvert.DeserializeObject<ConnectionInfo>(result.Data);
                });

                dipSocketClient.On("OnMessageReceived", (result) =>
                {
                    OnMessageReceived(result);
                });

                dipSocketClient.On("OnMessageError", (result) =>
                {
                    OnMessageError(result);
                });

                dipSocketClient.On("OnServerInfo", (result) =>
                {
                    OnServerInfo(result);
                });

                await dipSocketClient.StartAsync();
            }
            catch (Exception ex)
            {
                Errors.Add(new Error { Message = ex.Message, Verbose = ex.ToString() });
            }
        }
        
        private async void OnAddInfo(object args)
        {
            if(AddInfoName == null
                || string.IsNullOrWhiteSpace(AddInfoName))
            {
                Errors.Add(new Error { Message = "Specify the name of the channel you want to create.", Verbose = "Specify the name of the channel you want to create." });
                return;
            }

            try
            {
                var info = ServerInfos.FirstOrDefault(i => i.Name.Equals(AddInfoName));
                if (info != null)
                {
                    UserInfos.Add(info);
                }
                else
                {
                    var channel = InfoFactory.GetInfo(new ChannelInfo { Name = AddInfoName });
                    ServerInfos.Add(channel);
                    UserInfos.Add(channel);
                }

                var clientMessage = new Message { SenderConnectionId = User.Name, Data = AddInfoName, MessageType = MessageType.SubscribeToChannel };

                await dipSocketClient.SendMessageAsync(clientMessage);
            }
            catch (Exception ex)
            {
                Errors.Add(new Error { Message = ex.Message, Verbose = ex.ToString() });
            }
            finally
            {
                AddInfoName = string.Empty;
            }
        }

        public async void OnSendMessage(object arg)
        {
            try
            {
                if(SelectedInfo == null)
                {
                    return;
                }

                var clientMessage = new Message { SenderConnectionId = User.ConnectionId, RecipientConnectionId = SelectedInfo.ConnectionId, Data = Message, MessageType = MessageType.SendToClient };

                if (SelectedInfo is Channel)
                {
                    clientMessage.MessageType = MessageType.SendToChannel;
                }

                await dipSocketClient.SendMessageAsync(clientMessage);

                Message = string.Empty;
            }
            catch (Exception ex)
            {
                Errors.Add(new Error { Message = ex.Message, Verbose = ex.ToString() });
            }
        }

        private void OnRemoveItem(object item)
        {
            throw new NotImplementedException();
        }

        private void OnClearErrors(object args)
        {
            Errors.Clear();
        }

        private void DipSocketClientError(object sender, Exception ex)
        {
            AddError(new Error { Message = ex.Message, Verbose = ex.ToString() });
        }

        private void DipSocketClientClosed(object sender, EventArgs e)
        {
            AddError(new Error { Message = "Connection Closed" });
        }

        private void OnMessageReceived(Message message)
        {
            var serverInfo = ServerInfos.OfType<InfoDecorator>().FirstOrDefault(si => si.ConnectionId.Equals(message.RecipientConnectionId));
            if (serverInfo == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                serverInfo.Conversation.Add(new Comment { SentOn = message.SentOn, Sender = message.SenderConnectionId, Text = message.Data });
            });
        }

        private void OnMessageError(Message message)
        {
            var error = JsonConvert.DeserializeObject<string>(message.Data);
            AddError(new Error { Message = error });
        }

        private void OnServerInfo(Message message)
        {
            dispatcher.Invoke(
                            () =>
                            {
                                lock (serverInfosLock)
                                {
                                    var serverInfo = JsonConvert.DeserializeObject<ServerInfo>(message.Data);
                                    var allServerInfos = serverInfo.Channels.Cast<IInfo>()
                                                                            .Union(serverInfo.Connections.Where(c => !c.Name.Equals(User.Name)).Cast<IInfo>())
                                                                            .OrderBy(c => c.Name).ToList();

                                    var removals = ServerInfos.Where(c => !allServerInfos.Any(nc => nc.Name.Equals(c.Name))).ToList();
                                    foreach (var removal in removals)
                                    {
                                        ServerInfos.Remove(removal);
                                        var removeUserInfo = UserInfos.FirstOrDefault(i => i.Name.Equals(removal.Name));
                                        if (removeUserInfo != null)
                                        {
                                            UserInfos.Remove(removeUserInfo);
                                        }
                                    }

                                    var updates = (from c in ServerInfos.OfType<Channel>()
                                                   join ci in allServerInfos.OfType<ChannelInfo>()
                                                   on c.Name equals ci.Name
                                                   select c.UpdateConnections(ci)).ToList();

                                    var additions = allServerInfos.Where(a => !ServerInfos.Any(c => c.Name.Equals(a.Name))).ToList();
                                    if (additions.Any())
                                    {
                                        foreach (var addition in additions)
                                        {
                                            dispatcher.Invoke(
                                                () =>
                                                {
                                                    var info = InfoFactory.GetInfo(addition);
                                                    ServerInfos.Add(info);
                                                });
                                        }
                                    }
                                }
                            });
        }

        private void AddError(Error error)
        {
            lock(errorsLock)
            {
                Errors.Add(error);
            }
        }

        private void ErrorsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("HasErrors");
        }
    }
}