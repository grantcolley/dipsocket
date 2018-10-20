using Client.Command;
using Client.Model;
using System.Windows.Input;

namespace Client.ViewModel
{
    public class ChatViewModel : NotifyPropertyChangedBase
    {
        private string user;
        private string message;

        public ChatViewModel()
        {
            ConnectCommand = new ViewModelCommand(OnConnect);
        }

        public ICommand ConnectCommand { get; set; }

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

        private void OnConnect(object arg)
        {
            if(arg == null 
                || string.IsNullOrWhiteSpace(arg.ToString()))
            {
                return;
            }

            user = arg.ToString(); ;
        }
    }
}