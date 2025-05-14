using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using Lechebnik.Helpers;
using Lechebnik.Models;
using Lechebnik.Views;
using NLog;

namespace Lechebnik.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ObservableCollection<Order> _orders;
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand BackToMainCommand { get; }

        public OrdersViewModel()
        {
            Orders = new ObservableCollection<Order>();
            SearchCommand = new RelayCommand(SearchOrders);
            ViewDetailsCommand = new RelayCommand<Order>(ViewDetails);
            CancelOrderCommand = new RelayCommand<Order>(CancelOrder);
            BackToMainCommand = new RelayCommand(BackToMain);
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT o.ID, o.Пользователь_ID, o.Дата_заказа, s.Название as Статус, o.Дата_доставки, o.Сумма, o.Пункт_выдачи_ID, o.Способ_оплаты
                        FROM Заказы o
                        JOIN Статусы_заказов s ON o.Статус = s.ID
                        WHERE o.Пользователь_ID = @UserID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", App.CurrentUser.ID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            Orders.Clear();
                            while (reader.Read())
                            {
                                var order = new Order
                                {
                                    ID = reader.GetInt32(0),
                                    Пользователь_ID = reader.GetInt32(1),
                                    Дата_заказа = reader.GetDateTime(2),
                                    Статус = reader.GetString(3),
                                    Сумма = reader.GetDecimal(5),
                                    Пункт_выдачи_ID = reader.GetInt32(6),
                                    Способ_оплаты = reader.GetString(7)
                                };
                                if (reader.IsDBNull(4))
                                {
                                    order.Дата_доставки = null;
                                }
                                else
                                {
                                    order.Дата_доставки = reader.GetDateTime(4);
                                }
                                Orders.Add(order);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке заказов.");
                Logger.Error(ex, "Ошибка при загрузке заказов.");
            }
        }

        private void SearchOrders()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadOrders();
                return;
            }

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT o.ID, o.Пользователь_ID, o.Дата_заказа, s.Название as Статус, o.Дата_доставки, o.Сумма, o.Пункт_выдачи_ID, o.Способ_оплаты
                        FROM Заказы o
                        JOIN Статusy_заказов s ON o.Статус = s.ID
                        JOIN Детали_заказа d ON o.ID = d.Заказ_ID
                        JOIN Препараты p ON d.Препарат_ID = p.ID
                        WHERE o.Пользователь_ID = @UserID AND p.Название LIKE @SearchText";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", App.CurrentUser.ID);
                        command.Parameters.AddWithValue("@SearchText", $"%{SearchText}%");
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            Orders.Clear();
                            while (reader.Read())
                            {
                                var order = new Order
                                {
                                    ID = reader.GetInt32(0),
                                    Пользователь_ID = reader.GetInt32(1),
                                    Дата_заказа = reader.GetDateTime(2),
                                    Статус = reader.GetString(3),
                                    Сумма = reader.GetDecimal(5),
                                    Пункт_выдачи_ID = reader.GetInt32(6),
                                    Способ_оплаты = reader.GetString(7)
                                };
                                if (reader.IsDBNull(4))
                                {
                                    order.Дата_доставки = null;
                                }
                                else
                                {
                                    order.Дата_доставки = reader.GetDateTime(4);
                                }
                                Orders.Add(order);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при поиске заказов.");
                Logger.Error(ex, "Ошибка при поиске заказов.");
            }
        }

        private void ViewDetails(Order order)
        {
            OrderDetailsWindow detailsWindow = new OrderDetailsWindow();
            detailsWindow.DataContext = new OrderDetailsViewModel(order.ID);
            detailsWindow.ShowDialog();
        }

        private void CancelOrder(Order order)
        {
            MessageBoxResult result = MessageBox.Show("Отменить заказ?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No) return;

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "DELETE FROM Детали_заказа WHERE Заказ_ID = @OrderID; DELETE FROM Заказы WHERE ID = @OrderID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderID", order.ID);
                        command.ExecuteNonQuery();
                    }
                }
                Orders.Remove(order);
                MessageBox.Show("Заказ отменён.");
                Logger.Info($"Пользователь {App.CurrentUser.Логин} отменил заказ ID {order.ID}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отмене заказа.");
                Logger.Error(ex, "Ошибка при отмене заказа.");
            }
        }

        private void BackToMain()
        {
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}