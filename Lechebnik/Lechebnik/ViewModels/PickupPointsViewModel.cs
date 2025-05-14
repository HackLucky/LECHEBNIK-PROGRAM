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
    public class PickupPointsViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ObservableCollection<PickupPoint> _pickupPoints;
        public ObservableCollection<PickupPoint> PickupPoints
        {
            get => _pickupPoints;
            set => SetProperty(ref _pickupPoints, value);
        }

        public ICommand BackToMainCommand { get; }

        public PickupPointsViewModel()
        {
            PickupPoints = new ObservableCollection<PickupPoint>();
            BackToMainCommand = new RelayCommand(BackToMain);
            LoadPickupPoints();
        }

        private void LoadPickupPoints()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT ID, Название, Адрес, Время_работы FROM Пункты_выдачи";
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            PickupPoints.Clear();
                            while (reader.Read())
                            {
                                PickupPoints.Add(new PickupPoint
                                {
                                    ID = reader.GetInt32(0),
                                    Название = reader.GetString(1),
                                    Адрес = reader.GetString(2),
                                    Время_работы = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке пунктов выдачи.");
                Logger.Error(ex, "Ошибка при загрузке пунктов выдачи.");
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