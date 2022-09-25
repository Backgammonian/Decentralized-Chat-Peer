using System;
using System.Globalization;
using System.Windows.Data;
using Extensions;

namespace Converters
{
    public sealed class NetworkSpeedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var networkSpeed = System.Convert.ToInt64((double)value);
            return networkSpeed.GetSizeSuffix() + "/s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
