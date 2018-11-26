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
                    OnPropertyChanged("ChannelConnections");
                }
            }
        }

        public ObservableCollection<ConnectionInfo> ChannelConnections
        {
            get
            {
                if (SelectedInfo is Channel)
                {
                    return ((Channel)SelectedInfo).Connections;
                }

                return null;
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
                if (info == null)
                {
                    var channelInfo = new ChannelInfo { Name = AddInfoName };
                    info = InfoFactory.GetInfo(channelInfo);
                    ServerInfos.Add(info);
                }

                UserInfos.Add(info);

                SelectedInfo = (InfoDecorator)info;

                if (info is Channel)
                {
                    var clientMessage = new Message { SenderConnectionId = User.Name, Data = AddInfoName, MessageType = MessageType.SubscribeToChannel };

                    await dipSocketClient.SendMessageAsync(clientMessage);
                }
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

        private async void OnRemoveItem(object item)
        {
            var info = item as IInfo;

            if (info == null)
            {
                return;
            }

            try
            {
                UserInfos.Remove(info);

                if(info.Equals(SelectedInfo))
                {
                    SelectedInfo = null;
                }

                if (info is Channel)
                {
                    var clientMessage = new Message { SenderConnectionId = User.Name, Data = info.Name, MessageType = MessageType.UnsubscribeFromChannel };

                    await dipSocketClient.SendMessageAsync(clientMessage);
                }
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
            var recipient = UserInfos.OfType<InfoDecorator>().FirstOrDefault(si => si.ConnectionId.Equals(message.RecipientConnectionId));
            if (recipient == null)
            {
                return;
            }

            var senderName = string.Empty;

            if(recipient is Channel)
            {
                var sender = ((Channel)recipient).Connections.FirstOrDefault(si => si.ConnectionId.Equals(message.SenderConnectionId));
                if (sender != null)
                {
                    senderName = sender.Name;
                }
            }
            else
            {
                var sender = UserInfos.OfType<InfoDecorator>().FirstOrDefault(si => si.ConnectionId.Equals(message.SenderConnectionId));
                if(sender != null)
                {
                    senderName = sender.Name;
                }
            }

            if (string.IsNullOrWhiteSpace(senderName))
            {
                if (User.ConnectionId.Equals(message.SenderConnectionId))
                {
                    senderName = User.Name;
                }
                else
                {
                    AddError(new Error { Message = "Message received with no sender" });
                    return;
                }
            }

            dispatcher.Invoke(() =>
            {
                recipient.Conversation.Add(new Comment { SentOn = message.SentOn, Sender = senderName, Text = message.Data });
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