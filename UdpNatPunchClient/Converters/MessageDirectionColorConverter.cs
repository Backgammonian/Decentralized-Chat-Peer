using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using UdpNatPunchClient.Models;

namespace Converters
{
    public class MessageDirectionColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush _incomingMessageColor = new SolidColorBrush(Colors.LightGray);
        private static readonly SolidColorBrush _outcomingMessageColor = new SolidColorBrush(Colors.LightBlue);
        private static readonly SolidColorBrush _unforseenMessageColor = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((MessageDirection)value)
            {
                case MessageDirection.Incoming:
                    return _incomingMessageColor;

                case MessageDirection.Outgoing:
                    return _outcomingMessageColor;

                default:
                    return _unforseenMessageColor;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
