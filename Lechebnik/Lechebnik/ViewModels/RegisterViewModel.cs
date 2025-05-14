using System.Windows.Input;
using Lechebnik.Helpers;
using System.Data.SqlClient;
using System.Windows;
using NLog;
using System.Text.RegularExpressions;
using System;
using Lechebnik.Views;

namespace Lechebnik.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string Фамилия { get; set; }
        public string Имя { get; set; }
        public string Отчество { get; set; }
        public string Телефон { get; set; }
        public string Почта { get; set; }
        public string Логин { get; set; }
        public string Пароль { get; set; }
        public string Повторный_пароль { get; set; }

        public ICommand ЗарегистрироватьсяCommand { get; }
        public ICommand НазадCommand { get; }
        public ICommand ВыходCommand { get; }

        public RegisterViewModel()
        {
            ЗарегистрироватьсяCommand = new RelayCommand(Зарегистрироваться);
            НазадCommand = new RelayCommand(ОткрытьАвторизацию);
            ВыходCommand = new RelayCommand(Выход);
        }

        private void Зарегистрироваться()
        {
            // Валидация полей
            if (!ValidateInput())
                return;

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "INSERT INTO Пользователи (Логин, Пароль, Фамилия, Имя, Отчество, Телефон, Почта, Роль, Статус) " +
                                   "VALUES (@Логин, @Пароль, @Фамилия, @Имя, @Отчество, @Телефон, @Почта, 1, 1)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Логин", Логин);
                        command.Parameters.AddWithValue("@Пароль", PasswordHelper.HashPassword(Пароль));
                        command.Parameters.AddWithValue("@Фамилия", Фамилия);
                        command.Parameters.AddWithValue("@Имя", Имя);
                        command.Parameters.AddWithValue("@Отчество", Отчество ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Телефон", Телефон ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Почта", Почта ?? (object)DBNull.Value);
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Регистрация прошла успешно!");
                Logger.Info($"Успешная регистрация пользователя: {Логин}");
                ОткрытьАвторизацию();
            }
            catch (SqlException ex) when (ex.Number == 2627) // Уникальность логина нарушена
            {
                MessageBox.Show("Этот логин уже занят.");
                Logger.Warn($"Попытка регистрации с занятым логином: {Логин}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации.");
                Logger.Error(ex, "Ошибка при регистрации.");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Фамилия) || string.IsNullOrWhiteSpace(Имя) || string.IsNullOrWhiteSpace(Логин) ||
                string.IsNullOrWhiteSpace(Пароль) || string.IsNullOrWhiteSpace(Повторный_пароль))
            {
                MessageBox.Show("Заполните все обязательные поля.");
                return false;
            }

            if (Фамилия.Length > 50 || Имя.Length > 50 || (Отчество?.Length > 50) || Логин.Length > 50)
            {
                MessageBox.Show("Длина полей не должна превышать 50 символов.");
                return false;
            }

            if (!Regex.IsMatch(Фамилия, @"^[А-Яа-я]+$") || !Regex.IsMatch(Имя, @"^[А-Яа-я]+$") ||
                (Отчество != null && !Regex.IsMatch(Отчество, @"^[А-Яа-я]+$")))
            {
                MessageBox.Show("Фамилия, имя и отчество должны содержать только русские буквы.");
                return false;
            }

            if (Телефон != null && !Regex.IsMatch(Телефон, @"^\+7\d{10}$"))
            {
                MessageBox.Show("Телефон должен быть в формате +79991234567.");
                return false;
            }

            if (Почта != null && !Regex.IsMatch(Почта, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный адрес почты.");
                return false;
            }

            if (Пароль != Повторный_пароль)
            {
                MessageBox.Show("Пароли не совпадают.");
                return false;
            }

            if (Пароль.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов.");
                return false;
            }

            return true;
        }

        private void ОткрытьАвторизацию()
        {
            LoginWindow loginWindow = new LoginWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
        }

        private void Выход()
        {
            Logger.Info("Выход из программы.");
            Application.Current.Shutdown();
        }
    }
}