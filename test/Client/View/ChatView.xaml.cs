using Client.ViewModel;
using System;
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

            var login = new LoginView();
            login.ShowDialog();
            if(!login.DialogResult.HasValue
                || !login.DialogResult.Value)
            {
                Application.Current.Shutdown();
            }

            Title = login.UserName;

            ((ChatViewModel)DataContext).ConnectCommand.Execute(login.UserName);
        }

        private void MessageKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key.Equals(Key.Enter))
            {
                ((ChatViewModel)DataContext).SendMessageCommand.Execute(txtMsg.Text);
            }
        }

        private void WindowClosing(object sender, EventArgs e)
        {
            ((ChatViewModel)DataContext).DisconnectCommand.Execute(null);
        }
    }
}
