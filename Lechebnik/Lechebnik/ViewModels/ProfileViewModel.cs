using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Lechebnik.Helpers;
using Lechebnik.Models;
using Lechebnik.Views;
using NLog;

namespace Lechebnik.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public ICommand SaveChangesCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand BackToMainCommand { get; }

        public ProfileViewModel()
        {
            CurrentUser = new User
            {
                ID = App.CurrentUser.ID,
                Логин = App.CurrentUser.Логин,
                Фамилия = App.CurrentUser.Фамилия,
                Имя = App.CurrentUser.Имя,
                Отчество = App.CurrentUser.Отчество,
                Телефон = App.CurrentUser.Телефон,
                Почта = App.CurrentUser.Почта
            };
            SaveChangesCommand = new RelayCommand(SaveChanges);
            LogoutCommand = new RelayCommand(Logout);
            BackToMainCommand = new RelayCommand(BackToMain);
        }

        private void SaveChanges()
        {
            if (!ValidateInput())
                return;

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        UPDATE Пользователи
                        SET Фамилия = @Фамилия, Имя = @Имя, Отчество = @Отчество, Телефон = @Телефон, Почта = @Почта
                        WHERE ID = @UserID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Фамилия", CurrentUser.Фамилия);
                        command.Parameters.AddWithValue("@Имя", CurrentUser.Имя);
                        command.Parameters.AddWithValue("@Отчество", CurrentUser.Отчество ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Телефон", CurrentUser.Телефон ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Почта", CurrentUser.Почта ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@UserID", CurrentUser.ID);
                        command.ExecuteNonQuery();
                    }
                }
                App.CurrentUser = CurrentUser;
                MessageBox.Show("Изменения сохранены.");
                Logger.Info($"Пользователь {CurrentUser.Логин} обновил профиль.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении изменений.");
                Logger.Error(ex, "Ошибка при сохранении профиля.");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(CurrentUser.Фамилия) || string.IsNullOrWhiteSpace(CurrentUser.Имя))
            {
                MessageBox.Show("Фамилия и имя обязательны.");
                return false;
            }

            if (CurrentUser.Фамилия.Length > 50 || CurrentUser.Имя.Length > 50 || (CurrentUser.Отчество?.Length > 50))
            {
                MessageBox.Show("Длина полей не должна превышать 50 символов.");
                return false;
            }

            if (!Regex.IsMatch(CurrentUser.Фамилия, @"^[А-Яа-я]+$") || !Regex.IsMatch(CurrentUser.Имя, @"^[А-Яа-я]+$") ||
                (CurrentUser.Отчество != null && !Regex.IsMatch(CurrentUser.Отчество, @"^[А-Яа-я]+$")))
            {
                MessageBox.Show("Фамилия, имя и отчество должны содержать только русские буквы.");
                return false;
            }

            if (CurrentUser.Телефон != null && !Regex.IsMatch(CurrentUser.Телефон, @"^\+7\d{10}$"))
            {
                MessageBox.Show("Телефон должен быть в формате +79991234567.");
                return false;
            }

            if (CurrentUser.Почта != null && !Regex.IsMatch(CurrentUser.Почта, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный адрес почты.");
                return false;
            }

            return true;
        }

        private void Logout()
        {
            Logger.Info($"Пользователь {CurrentUser.Логин} вышел из системы.");
            App.CurrentUser = null;
            LoginWindow loginWindow = new LoginWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
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