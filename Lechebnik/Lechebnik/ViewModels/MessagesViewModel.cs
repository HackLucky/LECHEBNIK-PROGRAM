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
    public class MessagesViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ObservableCollection<Message> _messages;
        public ObservableCollection<Message> Messages
        {
            get => _messages;
            set => SetProperty(ref _messages, value);
        }

        private string _newMessage;
        public string NewMessage
        {
            get => _newMessage;
            set => SetProperty(ref _newMessage, value);
        }

        public ICommand SendMessageCommand { get; }
        public ICommand BackToMainCommand { get; }

        public MessagesViewModel()
        {
            Messages = new ObservableCollection<Message>();
            SendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
            BackToMainCommand = new RelayCommand(BackToMain);
            LoadMessages();
        }

        private void LoadMessages()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT ID, Отправитель_ID, Получатель_ID, Сообщение, Дата_отправки
                        FROM Сообщения
                        WHERE (Отправитель_ID = @UserID OR Получатель_ID = @UserID)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", App.CurrentUser.ID);
                        using (var reader = command.ExecuteReader())
                        {
                            Messages.Clear();
                            while (reader.Read())
                            {
                                Messages.Add(new Message
                                {
                                    ID = reader.GetInt32(0),
                                    Отправитель_ID = reader.GetInt32(1),
                                    Получатель_ID = reader.GetInt32(2),
                                    Сообщение = reader.GetString(3),
                                    Дата_отправки = reader.GetDateTime(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке сообщений.");
                Logger.Error(ex, "Ошибка при загрузке сообщений.");
            }
        }

        private bool CanSendMessage()
        {
            return !string.IsNullOrWhiteSpace(NewMessage);
        }

        private void SendMessage()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO Сообщения (Отправитель_ID, Получатель_ID, Сообщение, Дата_отправки)
                        VALUES (@SenderID, @ReceiverID, @Message, @SendDate)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SenderID", App.CurrentUser.ID);
                        command.Parameters.AddWithValue("@ReceiverID", 1); // Предполагается, что ID администратора = 1
                        command.Parameters.AddWithValue("@Message", NewMessage);
                        command.Parameters.AddWithValue("@SendDate", DateTime.Now);
                        command.ExecuteNonQuery();
                    }
                }
                Messages.Add(new Message
                {
                    Отправитель_ID = App.CurrentUser.ID,
                    Получатель_ID = 1,
                    Сообщение = NewMessage,
                    Дата_отправки = DateTime.Now
                });
                NewMessage = string.Empty;
                Logger.Info($"Пользователь {App.CurrentUser.Логин} отправил сообщение администратору.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке сообщения.");
                Logger.Error(ex, "Ошибка при отправке сообщения.");
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