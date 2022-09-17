namespace Extensions
{
    public static class CalculateSizeExtension
    {
        private static readonly string[] _sizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string GetSizeSuffix(this long value, int decimalPlaces = 1)
        {
            if (value < 0)
            {
                return "-" + GetSizeSuffix(-value, decimalPlaces);
            }

            int i = 0;
            decimal dValue = value;

            while (decimal.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, _sizeSuffixes[i]);
        }
    }
}