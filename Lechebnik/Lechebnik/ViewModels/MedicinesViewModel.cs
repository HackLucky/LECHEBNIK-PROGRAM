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
    public class MedicinesViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ObservableCollection<Medicine> _medicines;
        public ObservableCollection<Medicine> Medicines
        {
            get => _medicines;
            set => SetProperty(ref _medicines, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand BackToMainCommand { get; }

        public MedicinesViewModel()
        {
            Medicines = new ObservableCollection<Medicine>();
            SearchCommand = new RelayCommand(SearchMedicines);
            ViewDetailsCommand = new RelayCommand<Medicine>(OpenDetails);
            AddToCartCommand = new RelayCommand<Medicine>(OpenAddToCart);
            BackToMainCommand = new RelayCommand(BackToMain);
            LoadMedicines();
        }

        // Загрузка всех препаратов из базы данных
        private void LoadMedicines()
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT ID, Название, Требование_рецепта, Количество_на_складе, Цена FROM Препараты";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            Medicines.Clear();
                            while (reader.Read())
                            {
                                Medicines.Add(new Medicine
                                {
                                    ID = reader.GetInt32(0),
                                    Название = reader.GetString(1),
                                    Требование_рецепта = reader.GetBoolean(2),
                                    Количество_на_складе = reader.GetInt32(3),
                                    Цена = reader.GetDecimal(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке препаратов.");
                Logger.Error(ex, "Ошибка при загрузке препаратов.");
            }
        }

        // Поиск препаратов по введенному тексту
        private void SearchMedicines()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadMedicines();
                return;
            }

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT ID, Название, Требование_рецепта, Количество_на_складе, Цена FROM Препараты " +
                                   "WHERE Название LIKE @SearchText OR Применение_при_симптомах LIKE @SearchText";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SearchText", $"%{SearchText}%");
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            Medicines.Clear();
                            while (reader.Read())
                            {
                                Medicines.Add(new Medicine
                                {
                                    ID = reader.GetInt32(0),
                                    Название = reader.GetString(1),
                                    Требование_рецепта = reader.GetBoolean(2),
                                    Количество_на_складе = reader.GetInt32(3),
                                    Цена = reader.GetDecimal(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при поиске препаратов.");
                Logger.Error(ex, "Ошибка при поиске препаратов.");
            }
        }

        // Открытие окна с деталями препарата
        private void OpenDetails(Medicine medicine)
        {
            if (medicine == null) return;
            MedicineDetailsWindow detailsWindow = new MedicineDetailsWindow();
            detailsWindow.DataContext = new MedicineDetailsViewModel(medicine);
            detailsWindow.ShowDialog();
        }

        // Открытие окна добавления в корзину
        private void OpenAddToCart(Medicine medicine)
        {
            if (medicine == null) return;
            if (medicine.Количество_на_складе <= 0)
            {
                MessageBox.Show("Препарат отсутствует на складе.");
                return;
            }
            AddToCartWindow addToCartWindow = new AddToCartWindow();
            addToCartWindow.DataContext = new AddToCartViewModel(medicine);
            addToCartWindow.ShowDialog();
        }

        // Возврат в главное меню
        private void BackToMain()
        {
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}