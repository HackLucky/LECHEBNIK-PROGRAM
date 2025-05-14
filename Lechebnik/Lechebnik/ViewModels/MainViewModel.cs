using System.Windows.Input;
using Lechebnik.Models;
using System.Windows;
using NLog;
using Lechebnik.Helpers;
using Lechebnik.Views;

namespace Lechebnik.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public bool IsAdmin => CurrentUser?.Роль == 2;

        public ICommand ПрепаратыCommand { get; }
        public ICommand КорзинаCommand { get; }
        public ICommand ЗаказыCommand { get; }
        public ICommand ПунктыВыдачиCommand { get; }
        public ICommand ПрофильCommand { get; }
        public ICommand СвязьСАдминомCommand { get; }
        public ICommand ПанельАдминаCommand { get; }
        public ICommand ВыходCommand { get; }

        public MainViewModel()
        {
            CurrentUser = App.CurrentUser; // Получение текущего пользователя

            ПрепаратыCommand = new RelayCommand(ОткрытьПрепараты);
            КорзинаCommand = new RelayCommand(ОткрытьКорзину);
            ЗаказыCommand = new RelayCommand(ОткрытьЗаказы);
            ПунктыВыдачиCommand = new RelayCommand(ОткрытьПунктыВыдачи);
            ПрофильCommand = new RelayCommand(ОткрытьПрофиль);
            СвязьСАдминомCommand = new RelayCommand(ОткрытьСвязьСАдмином);
            ПанельАдминаCommand = new RelayCommand(ОткрытьПанельАдмина, () => IsAdmin);
            ВыходCommand = new RelayCommand(Выход);
        }

        private void ОткрытьПрепараты()
        {
            MedicinesWindow window = new MedicinesWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = window;
            window.Show();
        }

        private void ОткрытьКорзину()
        {
            CartWindow window = new CartWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = window;
            window.Show();
        }

        private void ОткрытьЗаказы()
        {
            OrdersWindow window = new OrdersWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = window;
            window.Show();
        }

        private void ОткрытьПунктыВыдачи()
        {
            PickupPointsWindow window = new PickupPointsWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = window;
            window.Show();
        }

        private void ОткрытьПрофиль()
        {
            ProfileWindow window = new ProfileWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = window;
            window.Show();
        }

        private void ОткрытьСвязьСАдмином()
        {
            MessagesWindow window = new MessagesWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = window;
            window.Show();
        }

        private void ОткрытьПанельАдмина()
        {
            AdminPanelWindow window = new AdminPanelWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = window;
            window.Show();
        }

        private void Выход()
        {
            Logger.Info($"Пользователь {CurrentUser.Логин} вышел из системы.");
            App.CurrentUser = null;
            LoginWindow loginWindow = new LoginWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
        }
    }
}