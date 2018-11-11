using Client.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace Client.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ChatView : Window
    {
        public ChatView()
        {
            InitializeComponent();
        }

        private void ConnectUserKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                ((ChatViewModel)DataContext).ConnectCommand.Execute(txtUser.Text);
            }
        }

        private void NewChannelKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                ((ChatViewModel)DataContext).NewChannelCommand.Execute(txtNewChannel.Text);
            }
        }

        private void MessageKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key.Equals(Key.Enter))
            {
                ((ChatViewModel)DataContext).SendMessage();
            }
        }
    }
}
