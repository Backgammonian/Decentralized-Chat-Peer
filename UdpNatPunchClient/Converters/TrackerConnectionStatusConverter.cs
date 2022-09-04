using System;
using System.Globalization;
using System.Windows.Data;
using System.Net;
using UdpNatPunchClient.Models;

namespace Converters
{
    public class TrackerConnectionStatusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (TrackerConnectionStatusType)values[0];

            var endPointString = string.Empty;
            var endPoint = values[1] as IPEndPoint;
            if (endPoint != null)
            {
                endPointString = endPoint.ToString();
            }

            switch (status)
            {
                case TrackerConnectionStatusType.None:
                    return string.Empty;

                case TrackerConnectionStatusType.TryingToConnect:
                    return $"Connecting to {endPointString}...";

                case TrackerConnectionStatusType.FailedToConnect:
                    return $"Couldn't connect to {endPointString} ❎";

                case TrackerConnectionStatusType.Connected:
                    return "Connected ✓";

                case TrackerConnectionStatusType.DisconnectFromTracker:
                    return $"Tracker {endPointString} disconnected";

                default:
                    return "---";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            //one-way binding only
            throw new NotImplementedException();
        }
    }
}
