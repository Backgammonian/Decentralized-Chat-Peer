using System;
using System.Windows.Media.Imaging;

namespace Extensions
{
    public static class BitmapImageExtensions
    {
        public static bool TryLoadBitmapImageFromPath(string path, out BitmapImage bitmapImage)
        {
            bitmapImage = new BitmapImage();

            try
            {
                var uri = new Uri(path, UriKind.Absolute);
                bitmapImage.BeginInit();
                bitmapImage.UriSource = uri;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
