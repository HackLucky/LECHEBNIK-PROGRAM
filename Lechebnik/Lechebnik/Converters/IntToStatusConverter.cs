using System;
using System.Globalization;
using System.Windows.Data;

namespace Lechebnik.Converters
{
    public class IntToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int statusId))
                return Binding.DoNothing;

            switch (statusId)
            {
                case 1: return "В сборке";
                case 2: return "Передан в доставку";
                case 3: return "Доставлен";
                case 4: return "Ожидание оплаты";
                default: return "Неизвестный статус";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (s == null) return 0;
            if (s == "В сборке") return 1;
            if (s == "Передан в доставку") return 2;
            if (s == "Доставлен") return 3;
            if (s == "Ожидание оплаты") return 4;
            return 0;
        }
    }
}
