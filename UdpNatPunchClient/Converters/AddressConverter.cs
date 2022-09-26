using System;
using System.Globalization;
using System.Windows.Data;
using System.Net;
using UdpNatPunchClient.Models;
using Networking;

namespace Converters
{
    public sealed class AddressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IPEndPoint endPoint)
            {
                return endPoint.ToString();
            }
            else
            if (value is EncryptedPeer peer)
            {
                return peer.ToString();
            }
            else
            if (value is UserModel user)
            {
                return user.EndPoint.ToString();
            }

            return "---";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
