using System;
using System.Globalization;
using System.Windows.Data;
using UdpNatPunchClient.Models;

namespace Converters
{
    public sealed class RecepientConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var recepient = value as PeerModel;

            if (recepient is UserModel)
            {
                var user = (UserModel)recepient;
                return $"Peer {user.ID} ({user.EndPoint})";
            }
            else
            if (recepient is TrackerModel)
            {
                var tracker = (TrackerModel)recepient;
                return $"Tracker ({tracker.EndPoint})";
            }

            return "---";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
