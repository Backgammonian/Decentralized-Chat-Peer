using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using UdpNatPunchClient.Models;

namespace Converters
{
    public sealed class MessageDirectionColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush _incomingMessageColor = new SolidColorBrush(Colors.LightGray);
        private static readonly SolidColorBrush _outcomingMessageColor = new SolidColorBrush(Colors.LightBlue);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((MessageDirection)value)
            {
                default:
                case MessageDirection.Incoming:
                    return _incomingMessageColor;

                case MessageDirection.Outgoing:
                    return _outcomingMessageColor;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
