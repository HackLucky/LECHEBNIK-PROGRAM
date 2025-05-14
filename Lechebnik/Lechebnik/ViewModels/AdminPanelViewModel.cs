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
    public class AdminPanelViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        private ObservableCollection<Order> _orders;
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        private ObservableCollection<Medicine> _medicines;
        public ObservableCollection<Medicine> Medicines
        {
            get => _medicines;
            set => SetProperty(ref _medicines, value);
        }

        public ICommand BlockUserCommand { get; }
        public ICommand UnblockUserCommand { get; }
        public ICommand UpdateOrderStatusCommand { get; }
        public ICommand AddMedicineCommand { get; }
        public ICommand BackToMainCommand { get; }

        public AdminPanelViewModel()
        {
            Users = new ObservableCollection<User>();
            Orders = new ObservableCollection<Order>();
            Medicines = new ObservableCollection<Medicine>();
            BlockUserCommand = new RelayCommand<User>(BlockUser);
            UnblockUserCommand = new RelayCommand<User>(UnblockUser);
            UpdateOrderStatusCommand = new RelayCommand<Order>(UpdateOrderStatus);
            AddMedicineCommand = new RelayCommand(AddMedicine);
            BackToMainCommand = new RelayCommand(BackToMain);
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    // Load users
                    string userQuery = "SELECT ID, Логин, Фамилия, Имя, Роль, Статус FROM Пользователи";
                    using (var command = new SqlCommand(userQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            Users.Clear();
                            while (reader.Read())
                            {
                                Users.Add(new User
                                {
                                    ID = reader.GetInt32(0),
                                    Логин = reader.GetString(1),
                                    Фамилия = reader.GetString(2),
                                    Имя = reader.GetString(3),
                                    Роль = reader.GetInt32(4),
                                    Статус = reader.GetInt32(5)
                                });
                            }
                        }
                    }

                    // Load orders
                    string orderQuery = @"
                        SELECT o.ID, o.Пользователь_ID, o.Дата_заказа, s.Название as Статус, o.Сумма
                        FROM Заказы o
                        JOIN Статусы_заказов s ON o.Статус = s.ID";
                    using (var command = new SqlCommand(orderQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            Orders.Clear();
                            while (reader.Read())
                            {
                                Orders.Add(new Order
                                {
                                    ID = reader.GetInt32(0),
                                    Пользователь_ID = reader.GetInt32(1),
                                    Дата_заказа = reader.GetDateTime(2),
                                    Статус = reader.GetString(3),
                                    Сумма = reader.GetDecimal(4)
                                });
                            }
                        }
                    }

                    // Load medicines
                    string medicineQuery = "SELECT ID, Название, Цена, Количество_на_складе FROM Препараты";
                    using (var command = new SqlCommand(medicineQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            Medicines.Clear();
                            while (reader.Read())
                            {
                                Medicines.Add(new Medicine
                                {
                                    ID = reader.GetInt32(0),
                                    Название = reader.GetString(1),
                                    Цена = reader.GetDecimal(2),
                                    Количество_на_складе = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных.");
                Logger.Error(ex, "Ошибка при загрузке данных администратора.");
            }
        }

        private void BlockUser(User user)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "UPDATE Пользователи SET Статус = 2 WHERE ID = @UserID";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", user.ID);
                        command.ExecuteNonQuery();
                    }
                }
                user.Статус = 2;
                Logger.Info($"Пользователь {user.Логин} заблокирован.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при блокировке пользователя.");
                Logger.Error(ex, "Ошибка при блокировке пользователя.");
            }
        }

        private void UnblockUser(User user)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "UPDATE Пользователи SET Статус = 1 WHERE ID = @UserID";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", user.ID);
                        command.ExecuteNonQuery();
                    }
                }
                user.Статус = 1;
                Logger.Info($"Пользователь {user.Логин} разблокирован.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при разблокировке пользователя.");
                Logger.Error(ex, "Ошибка при разблокировке пользователя.");
            }
        }

        private void UpdateOrderStatus(Order order)
        {
            MessageBox.Show("Функция изменения статуса заказа пока не реализована.");
        }

        private void AddMedicine()
        {
            MessageBox.Show("Функция добавления препарата пока не реализована.");
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