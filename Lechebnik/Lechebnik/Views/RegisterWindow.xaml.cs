using System.Windows;
using System.Windows.Controls;

namespace Lechebnik.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is Lechebnik.ViewModels.RegisterViewModel viewModel)
            {
                viewModel.Пароль = (sender as PasswordBox).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is Lechebnik.ViewModels.RegisterViewModel viewModel)
            {
                viewModel.Повторный_пароль = (sender as PasswordBox).Password;
            }
        }
    }
}