using System;
using System.Globalization;
using System.Windows.Data;

namespace Converters
{
    public sealed class ProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var progress = (decimal)value;
            return decimal.Round(progress * (decimal)100.0, 2) + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
