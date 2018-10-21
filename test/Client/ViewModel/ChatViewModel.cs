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

            messagesLock = new object();
            Messages = new ObservableCollection<Message>();
            BindingOperations.EnableCollectionSynchronization(Messages, messagesLock);

            clientWebSocketConnection = new ClientWebSocketConnection(@"ws://localhost:6000/chat");
        }

        public ICommand ConnectCommand { get; set; }

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

        public string Message
        {
            get { return message; }
            set
            {
                if(message != value)
                {
                    message = value;
                    OnPropertyChanged("Message");
                }
            }
        }

        public async void SendMessage()
        {

        }

        private async void OnConnect(object arg)
        {
            if(arg == null 
                || string.IsNullOrWhiteSpace(arg.ToString()))
            {
                return;
            }

            user = arg.ToString();

            try
            {
                clientWebSocketConnection.On("OnConnected", (newMessage) =>
                {
                    lock(messagesLock)
                    {
                        var message = JsonConvert.DeserializeObject<Message>(newMessage.ToString());
                        Messages.Add(message);
                    }
                });

                clientWebSocketConnection.On("method2", (newMessage) =>
                {
                    lock (messagesLock)
                    {
                        var message = JsonConvert.DeserializeObject<Message>(newMessage.ToString());
                        Messages.Add(message);
                    }
                });

                await clientWebSocketConnection.StartAsync();
            }
            catch(Exception ex)
            {
                // todo : exception handling
            }
        }
    }
}