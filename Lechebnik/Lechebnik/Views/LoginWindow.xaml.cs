using System.Windows;
using System.Windows.Controls;

namespace Lechebnik.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is Lechebnik.ViewModels.LoginViewModel viewModel)
            {
                viewModel.Пароль = (sender as PasswordBox).Password;
            }
        }
    }
}