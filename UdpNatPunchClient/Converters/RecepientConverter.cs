﻿using System;
using System.Globalization;
using System.Windows.Data;
using UdpNatPunchClient.Models;

namespace Converters
{
    public sealed class RecepientConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserModel user)
            {
                return $"Peer {user.ID} ({user.EndPoint})";
            }
            else
            if (value is TrackerModel tracker)
            {
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
