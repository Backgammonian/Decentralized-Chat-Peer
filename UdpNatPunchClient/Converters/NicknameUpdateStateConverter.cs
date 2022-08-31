using System;
using System.Globalization;
using System.Windows.Data;
using UdpNatPunchClient.Models;

namespace Converters
{
    public class NicknameUpdateStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (NicknameUpdateState)value;

            switch (state)
            {
                default:
                case NicknameUpdateState.None:
                    return string.Empty;
                    
                case NicknameUpdateState.Changing:
                    return "⏳";

                case NicknameUpdateState.Updated:
                    return "✓";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
