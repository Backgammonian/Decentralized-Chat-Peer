using System;
using System.Globalization;
using System.Windows.Data;
using System.Net;

namespace Converters
{
    public sealed class AddressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var endPoint = value as IPEndPoint;

            if (endPoint != null)
            {
                return endPoint.ToString();
            }

            return "---";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
