using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using UdpNatPunchClient.Models;

namespace Converters
{
    public sealed class MessageDirectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((MessageDirection)value)
            {
                default:
                case MessageDirection.Incoming:
                    return Visibility.Collapsed;

                case MessageDirection.Outgoing:
                    return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
