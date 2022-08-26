using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using UdpNatPunchClient.Models;

namespace Converters
{
    public class MessageDirectionAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((MessageDirection)value)
            {
                default:
                case MessageDirection.Incoming:
                    return HorizontalAlignment.Left;

                case MessageDirection.Outgoing:
                    return HorizontalAlignment.Right;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
