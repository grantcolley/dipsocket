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

        private void MessageKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key.Equals(Key.Enter))
            {
                ((ChatViewModel)DataContext).SendMessage();
            }
        }
    }
}
