using System;
using System.Drawing;
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

        public static bool TryLoadBitmapImageFromPath(string path, int width, int height, out BitmapImage bitmapImage)
        {
            bitmapImage = new BitmapImage();

            if (width <= 0 || height <= 0)
            {
                return false;
            }

            try
            {
                using var bitmap = (Bitmap)Image.FromFile(path);
                bitmapImage = bitmap.ResizeImageWithPreservedAspectRatio(width, height);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
