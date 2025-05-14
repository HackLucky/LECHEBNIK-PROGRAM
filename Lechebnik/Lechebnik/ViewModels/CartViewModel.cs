using System;
using System.Collections.Generic;
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
    public class CartViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ObservableCollection<CartItem> _cartItems;
        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand ChangeQuantityCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand BackToMainCommand { get; }
        public ICommand PlaceOrderCommand { get; }

        public CartViewModel()
        {
            CartItems = new ObservableCollection<CartItem>();
            SearchCommand = new RelayCommand(SearchCartItems);
            ViewDetailsCommand = new RelayCommand<CartItem>(ViewDetails);
            ChangeQuantityCommand = new RelayCommand<CartItem>(ChangeQuantity);
            RemoveItemCommand = new RelayCommand<CartItem>(RemoveItem);
            BackToMainCommand = new RelayCommand(BackToMain);
            PlaceOrderCommand = new RelayCommand(PlaceOrder);
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT c.ID, c.Пользователь_ID, c.Препарат_ID, c.Количество
                        FROM Корзина c
                        WHERE c.Пользователь_ID = @UserID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", App.CurrentUser.ID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            CartItems.Clear();
                            while (reader.Read())
                            {
                                CartItems.Add(new CartItem
                                {
                                    ID = reader.GetInt32(0),
                                    Пользователь_ID = reader.GetInt32(1),
                                    Препарат_ID = reader.GetInt32(2),
                                    Количество = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке корзины.");
                Logger.Error(ex, "Ошибка при загрузке корзины.");
            }
        }

        private void SearchCartItems()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadCartItems();
                return;
            }

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT c.ID, c.Пользователь_ID, c.Препарат_ID, c.Количество
                        FROM Корзина c
                        JOIN Препараты p ON c.Препарат_ID = p.ID
                        WHERE c.Пользователь_ID = @UserID AND p.Название LIKE @SearchText";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", App.CurrentUser.ID);
                        command.Parameters.AddWithValue("@SearchText", $"%{SearchText}%");
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            CartItems.Clear();
                            while (reader.Read())
                            {
                                CartItems.Add(new CartItem
                                {
                                    ID = reader.GetInt32(0),
                                    Пользователь_ID = reader.GetInt32(1),
                                    Препарат_ID = reader.GetInt32(2),
                                    Количество = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при поиске в корзине.");
                Logger.Error(ex, "Ошибка при поиске в корзине.");
            }
        }

        private void ViewDetails(CartItem item)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM Препараты WHERE ID = @MedicineID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MedicineID", item.Препарат_ID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Medicine medicine = new Medicine
                                {
                                    ID = reader.GetInt32(0),
                                    Название = reader.GetString(1),
                                    Требование_рецепта = reader.GetBoolean(2),
                                    Количество_на_складе = reader.GetInt32(3),
                                    Цена = reader.GetDecimal(4),
                                    Применение_при_симптомах = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    Способ_применения = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    Агрегатное_состояние = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    Тип_препарата = reader.IsDBNull(8) ? null : reader.GetString(8),
                                    Поставщик = reader.IsDBNull(9) ? null : reader.GetString(9),
                                    Страна_изготовления = reader.IsDBNull(10) ? null : reader.GetString(10)
                                };
                                MedicineDetailsWindow detailsWindow = new MedicineDetailsWindow();
                                detailsWindow.DataContext = new MedicineDetailsViewModel(medicine);
                                detailsWindow.ShowDialog();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке деталей препарата.");
                Logger.Error(ex, "Ошибка при загрузке деталей препарата.");
            }
        }

        private void ChangeQuantity(CartItem item)
        {
            AddToCartWindow addToCartWindow = new AddToCartWindow();
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Препараты WHERE ID = @MedicineID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MedicineID", item.Препарат_ID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Medicine medicine = new Medicine
                            {
                                ID = reader.GetInt32(0),
                                Название = reader.GetString(1),
                                Требование_рецепта = reader.GetBoolean(2),
                                Количество_на_складе = reader.GetInt32(3),
                                Цена = reader.GetDecimal(4)
                            };
                            addToCartWindow.DataContext = new AddToCartViewModel(medicine);
                            addToCartWindow.ShowDialog();
                        }
                    }
                }
            }
            LoadCartItems();
        }

        private void RemoveItem(CartItem item)
        {
            MessageBoxResult result = MessageBox.Show("Удалить препарат из корзины?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No) return;

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "DELETE FROM Корзина WHERE ID = @CartItemID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CartItemID", item.ID);
                        command.ExecuteNonQuery();
                    }
                }
                CartItems.Remove(item);
                MessageBox.Show("Препарат удалён из корзины.");
                Logger.Info($"Пользователь {App.CurrentUser.Логин} удалил элемент корзины ID {item.ID}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении из корзины.");
                Logger.Error(ex, "Ошибка при удалении из корзины.");
            }
        }

        private void BackToMain()
        {
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }

        private void PlaceOrder()
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста.");
                return;
            }

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        // Create new order
                        string orderQuery = @"
                            INSERT INTO Заказы (Пользователь_ID, Дата_заказа, Статус, Сумма, Пункт_выдачи_ID, Способ_оплаты)
                            OUTPUT INSERTED.ID
                            VALUES (@UserID, @OrderDate, @Status, @Total, @PickupPointID, @PaymentMethod)";
                        int orderId;
                        using (var command = new SqlCommand(orderQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@UserID", App.CurrentUser.ID);
                            command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                            command.Parameters.AddWithValue("@Status", (object)DBNull.Value); // Статус будет установлен после оплаты
                            command.Parameters.AddWithValue("@Total", 0); // Сумма будет обновлена позже
                            command.Parameters.AddWithValue("@PickupPointID", 1); // Предполагаемый пункт выдачи
                            command.Parameters.AddWithValue("@PaymentMethod", "Картой");
                            orderId = (int)command.ExecuteScalar();
                        }

                        // Add order items
                        decimal total = 0;
                        foreach (var item in CartItems)
                        {
                            string medicineQuery = "SELECT Цена, Количество_на_складе FROM Препараты WHERE ID = @MedicineID";
                            decimal price;
                            int stock;
                            using (var command = new SqlCommand(medicineQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@MedicineID", item.Препарат_ID);
                                using (var reader = command.ExecuteReader())
                                {
                                    if (!reader.Read())
                                        throw new Exception("Препарат не найден.");
                                    price = reader.GetDecimal(0);
                                    stock = reader.GetInt32(1);
                                }
                            }

                            if (item.Количество > stock)
                                throw new Exception($"Недостаточно препарата ID {item.Препарат_ID} на складе.");

                            string detailQuery = @"
                                INSERT INTO Детали_заказа (Заказ_ID, Препарат_ID, Количество, Цена)
                                VALUES (@OrderID, @MedicineID, @Quantity, @Price)";
                            using (var command = new SqlCommand(detailQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@OrderID", orderId);
                                command.Parameters.AddWithValue("@MedicineID", item.Препарат_ID);
                                command.Parameters.AddWithValue("@Quantity", item.Количество);
                                command.Parameters.AddWithValue("@Price", price);
                                command.ExecuteNonQuery();
                            }

                            // Update stock
                            string updateStockQuery = "UPDATE Препараты SET Количество_на_складе = Количество_на_складе - @Quantity WHERE ID = @MedicineID";
                            using (var command = new SqlCommand(updateStockQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Quantity", item.Количество);
                                command.Parameters.AddWithValue("@MedicineID", item.Препарат_ID);
                                command.ExecuteNonQuery();
                            }

                            total += price * item.Количество;
                        }

                        // Update order total
                        string updateTotalQuery = "UPDATE Заказы SET Сумма = @Total WHERE ID = @OrderID";
                        using (var command = new SqlCommand(updateTotalQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Total", total);
                            command.Parameters.AddWithValue("@OrderID", orderId);
                            command.ExecuteNonQuery();
                        }

                        // Clear cart
                        string clearCartQuery = "DELETE FROM Корзина WHERE Пользователь_ID = @UserID";
                        using (var command = new SqlCommand(clearCartQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@UserID", App.CurrentUser.ID);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        CartItems.Clear();

                        // Create order object for payment
                        Order order = new Order
                        {
                            ID = orderId,
                            Пользователь_ID = App.CurrentUser.ID,
                            Дата_заказа = DateTime.Now,
                            Сумма = total,
                            Пункт_выдачи_ID = 1,
                            Способ_оплаты = "Картой",
                            Items = new List<OrderItem>()
                        };

                        // Load order items for payment
                        string itemsQuery = @"
                            SELECT oi.Препарат_ID, oi.Количество, oi.Цена, m.Название
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
                                    order.Items.Add(new OrderItem
                                    {
                                        Препарат_ID = reader.GetInt32(0),
                                        Количество = reader.GetInt32(1),
                                        Цена = reader.GetDecimal(2),
                                        Название = reader.GetString(3)
                                    });
                                }
                            }
                        }

                        Logger.Info($"Пользователь {App.CurrentUser.Логин} оформил заказ ID {orderId}.");
                        PaymentWindow paymentWindow = new PaymentWindow();
                        paymentWindow.DataContext = new PaymentViewModel(order);
                        Application.Current.MainWindow.Close();
                        Application.Current.MainWindow = paymentWindow;
                        paymentWindow.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при оформлении заказа.");
                Logger.Error(ex, "Ошибка при оформлении заказа.");
            }
        }
    }
}