using LoginPage.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace LoginPage.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {

        public Login()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as LoginViewModel;
            if (viewModel != null)
            {
                viewModel.Password.Value = passwordBox.Password;
            }
        }
    }
}
