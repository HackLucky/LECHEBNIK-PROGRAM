using System.Windows;
using System.Windows.Input;
using Lechebnik.Helpers;
using Lechebnik.Views;

namespace Lechebnik.ViewModels
{
    public class GoToCartViewModel : BaseViewModel
    {
        public ICommand GoToCartCommand { get; }
        public ICommand ContinueShoppingCommand { get; }

        public GoToCartViewModel()
        {
            GoToCartCommand = new RelayCommand(GoToCart);
            ContinueShoppingCommand = new RelayCommand(ContinueShopping);
        }

        private void GoToCart()
        {
            CartWindow cartWindow = new CartWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = cartWindow;
            cartWindow.Show();
        }

        private void ContinueShopping()
        {
            Application.Current.Windows[Application.Current.Windows.Count - 1].Close();
        }
    }
}