using System.Windows.Input;
using Lechebnik.Helpers;
using Lechebnik.Models;
using System.Data.SqlClient;
using System.Windows;
using NLog;
using System;
using Lechebnik.Views;

namespace Lechebnik.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string _логин;
        public string Логин
        {
            get => _логин;
            set => SetProperty(ref _логин, value);
        }

        private string _пароль;
        public string Пароль
        {
            get => _пароль;
            set => SetProperty(ref _пароль, value);
        }

        public ICommand ВойтиCommand { get; }
        public ICommand РегистрацияCommand { get; }
        public ICommand ВыходCommand { get; }

        public LoginViewModel()
        {
            ВойтиCommand = new RelayCommand(Войти);
            РегистрацияCommand = new RelayCommand(ОткрытьРегистрацию);
            ВыходCommand = new RelayCommand(Выход);
        }

        private void Войти()
        {
            if (string.IsNullOrWhiteSpace(Логин) || string.IsNullOrWhiteSpace(Пароль))
            {
                MessageBox.Show("Введите логин и пароль.");
                Logger.Warn("Попытка входа с пустыми полями.");
                return;
            }

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT ID, Логин, Роль, Статус FROM Пользователи WHERE Логин = @Логин AND Пароль = @Пароль";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Логин", Логин);
                        command.Parameters.AddWithValue("@Пароль", PasswordHelper.HashPassword(Пароль));
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                User user = new User
                                {
                                    ID = reader.GetInt32(0),
                                    Логин = reader.GetString(1),
                                    Роль = reader.GetInt32(2),
                                    Статус = reader.GetInt32(3)
                                };

                                if (user.Статус == 2) // Заблокирован
                                {
                                    MessageBox.Show("Ваш аккаунт заблокирован.");
                                    Logger.Warn($"Попытка входа заблокированного пользователя: {Логин}");
                                    return;
                                }

                                App.CurrentUser = user; // Сохранение текущего пользователя
                                Logger.Info($"Успешный вход пользователя: {Логин}");
                                MainWindow mainWindow = new MainWindow();
                                Application.Current.MainWindow.Close();
                                Application.Current.MainWindow = mainWindow;
                                mainWindow.Show();
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль.");
                                Logger.Warn($"Неудачная попытка входа: {Логин}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных.");
                Logger.Error(ex, "Ошибка при авторизации.");
            }
        }

        private void ОткрытьРегистрацию()
        {
            RegisterWindow registerWindow = new RegisterWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = registerWindow;
            registerWindow.Show();
        }

        private void Выход()
        {
            Logger.Info("Выход из программы.");
            Application.Current.Shutdown();
        }
    }
}