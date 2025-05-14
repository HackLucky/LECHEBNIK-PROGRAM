using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using Lechebnik.Helpers;
using Lechebnik.Models;
using NLog;

namespace Lechebnik.ViewModels
{
    public class AddToCartViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Medicine _medicine;
        public Medicine Medicine
        {
            get => _medicine;
            set => SetProperty(ref _medicine, value);
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public int MaxQuantity => Math.Min(25, Medicine.Количество_на_складе);

        public ICommand AddToCartCommand { get; }
        public ICommand CancelCommand { get; }

        public AddToCartViewModel(Medicine medicine)
        {
            Medicine = medicine;
            Quantity = 1; // Значение по умолчанию
            AddToCartCommand = new RelayCommand(AddToCart, CanAddToCart);
            CancelCommand = new RelayCommand(Cancel);
        }

        // Проверка возможности добавления в корзину
        private bool CanAddToCart()
        {
            return Quantity > 0 && Quantity <= MaxQuantity;
        }

        // Добавление препарата в корзину
        private void AddToCart()
        {
            if (Medicine.Требование_рецепта)
            {
                MessageBoxResult result = MessageBox.Show("Этот препарат требует рецепт. Вы уверены, что хотите продолжить?", "Предупреждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "INSERT INTO Корзина (Пользователь_ID, Препарат_ID, Количество) VALUES (@UserID, @MedicineID, @Quantity)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", App.CurrentUser.ID);
                        command.Parameters.AddWithValue("@MedicineID", Medicine.ID);
                        command.Parameters.AddWithValue("@Quantity", Quantity);
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Препарат добавлен в корзину.");
                Logger.Info($"Пользователь {App.CurrentUser.Логин} добавил препарат {Medicine.Название} в корзину.");
                Cancel(); // Закрываем окно после успешного добавления
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении в корзину.");
                Logger.Error(ex, "Ошибка при добавлении в корзину.");
            }
        }

        // Закрытие окна
        private void Cancel()
        {
            Application.Current.Windows[Application.Current.Windows.Count - 1].Close();
        }
    }
}