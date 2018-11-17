using System.Windows;

namespace Client.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        public string UserName { get { return userName.Text; } }

        private void btnOkClick(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(userName.Text))
            {
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
