using Client.Command;
using Client.Model;
using DipSocket;
using DipSocket.Client;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Client.ViewModel
{
    public class ChatViewModel : NotifyPropertyChangedBase
    {
        private string user;
        private string message;
        private bool isConnected;
        private object messagesLock;
        private ClientWebSocketConnection clientWebSocketConnection;

        public ChatViewModel()
        {
            ConnectCommand = new ViewModelCommand(OnConnect);
            NewChannelCommand = new ViewModelCommand(OnNewChannel);

            messagesLock = new object();
            Messages = new ObservableCollection<Message>();
            BindingOperations.EnableCollectionSynchronization(Messages, messagesLock);

            clientWebSocketConnection = new ClientWebSocketConnection(@"ws://localhost:6000/chat");
        }

        public ICommand ConnectCommand { get; set; }
        public ICommand NewChannelCommand { get; set; }

        public ObservableCollection<Message> Messages { get; }

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

        public async void SendMessage()
        {

        }

        private async void OnConnect(object arg)
        {
            if (arg == null
                || string.IsNullOrWhiteSpace(arg.ToString()))
            {
                return;
            }

            user = arg.ToString();

            try
            {
                clientWebSocketConnection.On("OnConnected", (newMessage) =>
                {
                    lock (messagesLock)
                    {
                        var message = JsonConvert.DeserializeObject<Message>(newMessage.ToString());
                        Messages.Add(message);
                        IsConnected = true;
                    }
                });

                clientWebSocketConnection.On("OnMessageReceived", (newMessage) =>
                {
                    lock (messagesLock)
                    {
                        var message = JsonConvert.DeserializeObject<Message>(newMessage.ToString());
                        Messages.Add(message);
                    }
                });

                clientWebSocketConnection.On("OnChannelUpdate", (newMessage) =>
                {
                    lock (messagesLock)
                    {
                        var message = JsonConvert.DeserializeObject<Message>(newMessage.ToString());
                        Messages.Add(message);
                    }
                });

                await clientWebSocketConnection.StartAsync();
            }
            catch (Exception ex)
            {
                // todo : exception handling
            }
        }

        private async void OnNewChannel(object arg)
        {

        }
    }
}