using System;
using System.Globalization;
using System.Windows.Data;
using Extensions;

namespace Converters
{
    public sealed class BytesToMegabytesConverter : IValueConverter
    {
        public string ConvertSize(long size)
        {
            return size.GetSizeSuffix();
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