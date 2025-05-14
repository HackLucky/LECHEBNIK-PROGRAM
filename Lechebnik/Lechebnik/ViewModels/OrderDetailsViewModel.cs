using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using Lechebnik.Helpers;
using Lechebnik.Models;
using NLog;

namespace Lechebnik.ViewModels
{
    public class OrderDetailsViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Order _currentOrder;
        public Order CurrentOrder
        {
            get => _currentOrder;
            set => SetProperty(ref _currentOrder, value);
        }

        private ObservableCollection<OrderItem> _orderItems;
        public ObservableCollection<OrderItem> OrderItems
        {
            get => _orderItems;
            set => SetProperty(ref _orderItems, value);
        }

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public OrderDetailsViewModel(int orderId)
        {
            OrderItems = new ObservableCollection<OrderItem>();
            LoadOrderDetails(orderId);
        }

        private void LoadOrderDetails(int orderId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    // Load order basic info
                    string orderQuery = @"
                        SELECT o.ID, o.Пользователь_ID, o.Дата_заказа, s.Название as Статус
                        FROM Заказы o
                        JOIN Статусы_заказов s ON o.Статус = s.ID
                        WHERE o.ID = @OrderID";
                    using (var command = new SqlCommand(orderQuery, connection))
                    {
                        command.Parameters.AddWithValue("@OrderID", orderId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CurrentOrder = new Order
                                {
                                    ID = reader.GetInt32(0),
                                    Пользователь_ID = reader.GetInt32(1),
                                    Дата_заказа = reader.GetDateTime(2),
                                    Статус = reader.GetString(3),
                                };
                            }
                        }
                    }

                    // Load order items
                    string itemsQuery = @"
                        SELECT oi.ID, oi.Препарат_ID, oi.Количество, oi.Цена, m.Название
                        FROM Детали_заказа oi
                        JOIN Препараты m ON oi.Препарат_ID = m.ID
                        WHERE oi.Заказ_ID = @OrderID";
                    using (var command = new SqlCommand(itemsQuery, connection))
                    {
                        command.Parameters.AddWithValue("@OrderID", orderId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new OrderItem
                                {
                                    ID = reader.GetInt32(0),
                                    Препарат_ID = reader.GetInt32(1),
                                    Количество = reader.GetInt32(2),
                                    Цена = reader.GetDecimal(3),
                                    Название = reader.GetString(4)
                                };
                                OrderItems.Add(item);
                                TotalAmount += item.Количество * item.Цена;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке деталей заказа.");
                Logger.Error(ex, "Ошибка при загрузке деталей заказа.");
            }
        }
    }
}