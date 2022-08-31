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

        public string ConvertTime(DateTime time)
        {
            return string.Format("{0}:{1}:{2}, {3} {4} {5}",
                time.Hour,
                Format(time.Minute),
                Format(time.Second),
                time.Day,
                _monthAbbreviations[time.Month - 1],
                time.Year);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertTime((DateTime)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
