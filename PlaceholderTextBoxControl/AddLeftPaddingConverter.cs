using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ControlConverters
{
    public sealed class AddLeftPaddingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Thickness padding))
            {
                return value;
            }

            if (!double.TryParse(parameter.ToString(), out double amount))
            {
                return value;
            }

            padding.Left += amount;

            return padding;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
