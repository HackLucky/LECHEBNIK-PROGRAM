using System;
using System.Globalization;
using System.Windows.Data;

namespace Lechebnik.Converters
{
    public class UserStatusConverter : IValueConverter
    {
        // 1 - Активен; 2 - Заблокирован
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int statusId)) return Binding.DoNothing;
            switch (statusId)
            {
                case 1: return "Активен";
                case 2: return "Заблокирован";
                default: return "Неизвестен";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (s == "Активен") return 1;
            if (s == "Заблокирован") return 2;
            return 0;
        }
    }
}
