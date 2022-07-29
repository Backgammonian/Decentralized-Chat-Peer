using System;
using System.Globalization;
using System.Windows.Data;
using UdpNatPunchClient.Models;

namespace Converters
{
    public class DeliveryStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((DeliveryState)value)
            {
                default:
                case DeliveryState.NotDelivered:
                    return string.Empty;

                case DeliveryState.Delivered:
                    return "✓";

                case DeliveryState.ReadAndDelivered:
                    return "✓✓";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
