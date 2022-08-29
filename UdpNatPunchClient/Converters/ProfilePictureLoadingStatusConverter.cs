using System;
using System.Globalization;
using System.Windows.Data;
using UdpNatPunchClient.Models;

namespace Converters
{
    public class ProfilePictureLoadingStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (ProfilePictureLoadingStatusType)value;

            switch (status)
            {
                case ProfilePictureLoadingStatusType.None:
                    return string.Empty;

                case ProfilePictureLoadingStatusType.Loading:
                    return "Loading... 🔄";

                case ProfilePictureLoadingStatusType.ErrorOccurred:
                    return "Can't load picture ❎";

                case ProfilePictureLoadingStatusType.Completed:
                    return "Picture is loaded ✓";

                default:
                    return "---";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
