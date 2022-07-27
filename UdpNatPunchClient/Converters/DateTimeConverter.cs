using System;
using System.Globalization;
using System.Windows.Data;

namespace Converters
{
    public class DateTimeConverter : IValueConverter
    {
        private readonly string[] _monthAbbreviations = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private string Format(int number)
        {
            return number.ToString().Length == 1 ? "0" + number : "" + number;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = (DateTime)value;
            return string.Format("{0}:{1}:{2}, {3} {4} {5}",
                dateTime.Hour,
                Format(dateTime.Minute),
                Format(dateTime.Second),
                dateTime.Day,
                _monthAbbreviations[dateTime.Month - 1],
                dateTime.Year);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
