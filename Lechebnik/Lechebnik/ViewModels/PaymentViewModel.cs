using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using Lechebnik.Helpers;
using Lechebnik.Models;
using Lechebnik.Views;
using NLog;

namespace Lechebnik.ViewModels
{
    public class PaymentViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Order _currentOrder;
        public Order CurrentOrder
        {
            get => _currentOrder;
            set => SetProperty(ref _currentOrder, value);
        }

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        private string _selectedPaymentMethod;
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set => SetProperty(ref _selectedPaymentMethod, value);
        }

        public string[] PaymentMethods => new[] { "Картой онлайн", "Наличными при получении" };

        public ICommand ConfirmPaymentCommand { get; }
        public ICommand CancelCommand { get; }

        public PaymentViewModel(Order order)
        {
            CurrentOrder = order;
            LoadOrderItems();
            TotalAmount = CalculateTotalAmount();
            SelectedPaymentMethod = PaymentMethods[0];
            ConfirmPaymentCommand = new RelayCommand(ConfirmPayment, CanConfirmPayment);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadOrderItems()
        {
            try
            {
                CurrentOrder.Items = new List<OrderItem>();
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT oi.Препарат_ID, oi.Количество, oi.Цена, m.Название
                        FROM Детали_заказа oi
                        JOIN Препараты m ON oi.Препарат_ID = m.ID
                        WHERE oi.Заказ_ID = @OrderID";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderID", CurrentOrder.ID);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CurrentOrder.Items.Add(new OrderItem
                                {
                                    Препарат_ID = reader.GetInt32(0),
                                    Количество = reader.GetInt32(1),
                                    Цена = reader.GetDecimal(2),
                                    Название = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке элементов заказа.");
                Logger.Error(ex, "Ошибка при загрузке элементов заказа.");
            }
        }

        private decimal CalculateTotalAmount()
        {
            decimal total = 0;
            if (CurrentOrder.Items != null)
            {
                foreach (var item in CurrentOrder.Items)
                {
                    total += item.Цена * item.Количество;
                }
            }
            return total;
        }

        private bool CanConfirmPayment()
        {
            return !string.IsNullOrEmpty(SelectedPaymentMethod);
        }

        private void ConfirmPayment()
        {
            try
            {
                if (SelectedPaymentMethod == "Картой онлайн")
                {
                    MessageBox.Show("Оплата картой прошла успешно!");
                }
                else
                {
                    MessageBox.Show("Оплата будет произведена наличными при получении.");
                }

                CurrentOrder.Статус = "Оплачен";
                UpdateOrderStatusInDatabase();

                Logger.Info($"Пользователь {App.CurrentUser.Логин} оплатил заказ ID {CurrentOrder.ID}. Метод оплаты: {SelectedPaymentMethod}");

                MainWindow mainWindow = new MainWindow();
                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обработке оплаты.");
                Logger.Error(ex, "Ошибка при обработке оплаты.");
            }
        }

        private void UpdateOrderStatusInDatabase()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        UPDATE Заказы
                        SET Статус = (SELECT ID FROM Статусы_заказов WHERE Название = @Status)
                        WHERE ID = @OrderID";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", CurrentOrder.Статус);
                        command.Parameters.AddWithValue("@OrderID", CurrentOrder.ID);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка при обновлении статуса заказа.");
                throw;
            }
        }

        private void Cancel()
        {
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}