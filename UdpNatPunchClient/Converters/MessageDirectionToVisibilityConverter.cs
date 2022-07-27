using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using UdpNatPunchClient.Models;

namespace Converters
{
    public class MessageDirectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((MessageDirection)value)
            {
                default:
                case MessageDirection.Incoming:
                    return Visibility.Collapsed;

                case MessageDirection.Outcoming:
                    return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
