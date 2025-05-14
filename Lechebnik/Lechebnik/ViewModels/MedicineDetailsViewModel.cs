using Lechebnik.Models;

namespace Lechebnik.ViewModels
{
    public class MedicineDetailsViewModel : BaseViewModel
    {
        private Medicine _medicine;
        public Medicine Medicine
        {
            get => _medicine;
            set => SetProperty(ref _medicine, value);
        }

        public MedicineDetailsViewModel(Medicine medicine)
        {
            Medicine = medicine;
        }
    }
}