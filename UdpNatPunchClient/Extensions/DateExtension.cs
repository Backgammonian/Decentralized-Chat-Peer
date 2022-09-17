using System;

namespace Extensions
{
    public static class DateExtension
    {
        private static readonly string[] _monthAbbreviations = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private static string Format(int number)
        {
            var numberString = number.ToString();
            return numberString.Length == 1 ? "0" + number : numberString;
        }

        public static string ConvertTime(this DateTime dateTime)
        {
            return string.Format("{0}:{1}:{2}, {3} {4} {5}",
                dateTime.Hour,
                Format(dateTime.Minute),
                Format(dateTime.Second),
                dateTime.Day,
                _monthAbbreviations[dateTime.Month - 1],
                dateTime.Year);
        }
    }
}
