using Client.Command;
using Client.Model;
using DipSocket;
using DipSocket.Client;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Client.ViewModel
{
    public class ChatViewModel : NotifyPropertyChangedBase
    {
        private string connectionMessage;
        private bool isConnected;
        private object messagesLock;
        private ClientWebSocketConnection clientWebSocketConnection;

        public ChatViewModel()
        {
            ConnectCommand = new ViewModelCommand(OnConnect);
            NewChannelCommand = new ViewModelCommand(OnNewChannel);
            ClearErrorsCommand = new ViewModelCommand(OnClearErrors);

            messagesLock = new object();
            Messages = new ObservableCollection<Message>();

            Errors = new ObservableCollection<Error>();
            Errors.CollectionChanged += ErrorsCollectionChanged;

            BindingOperations.EnableCollectionSynchronization(Messages, messagesLock);
        }

        public ICommand ConnectCommand { get; set; }
        public ICommand NewChannelCommand { get; set; }
        public ICommand ClearErrorsCommand { get; set; }

        public ObservableCollection<Message> Messages { get; }
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
        
        public async void SendMessage()
        {

        }

        private async void OnConnect(object arg)
        {
            if (arg == null
                || string.IsNullOrWhiteSpace(arg.ToString()))
            {
                Errors.Add(new Error { Message = "A user name is required to connect to Chat", Verbose = "A user name is required to connect to Chat" });
                return;
            }

            var user = arg.ToString();

            try
            {
                clientWebSocketConnection = new ClientWebSocketConnection(@"ws://localhost:6000/chat");

                clientWebSocketConnection.On("OnConnected", (result) =>
                {
                    lock (messagesLock)
                    {
                        var message = (Message)result;
                        ConnectionMessage = $"{message.SentOn} {message.SentBy} {message.Data}";
                        IsConnected = true;
                    }
                });

                clientWebSocketConnection.On("OnMessageReceived", (result) =>
                {
                    lock (messagesLock)
                    {
                        Messages.Add((Message)result);
                    }
                });

                clientWebSocketConnection.On("OnChannelUpdate", (result) =>
                {
                    lock (messagesLock)
                    {

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

        private async void OnNewChannel(object arg)
        {

        }

        private async void OnClearErrors(object args)
        {
            Errors.Clear();
        }

        private void ErrorsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("HasErrors");
        }
    }
}