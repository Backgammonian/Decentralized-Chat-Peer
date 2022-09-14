using System;
using System.Globalization;
using System.Windows.Data;

namespace Converters
{
    public sealed class BytesToMegabytesConverter : IValueConverter
    {
        private readonly string[] _sizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        private string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (value < 0)
            {
                return "-" + SizeSuffix(-value, decimalPlaces);
            }

            int i = 0;
            decimal dValue = value;
            while (decimal.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, _sizeSuffixes[i]);
        }

        public string ConvertSize(long size)
        {
            return SizeSuffix(size);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertSize((long)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}