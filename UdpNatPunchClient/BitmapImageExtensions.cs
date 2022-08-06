using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace UdpNatPunchClient
{
    public static class BitmapImageExtensions
    {
        public const string EmptyImageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+ip1sAAAAASUVORK5CYII=";
        public static readonly BitmapImage EmptyImage = ConvertBase64ToBitmapImage(EmptyImageBase64);

        private static BitmapImage ConvertBase64ToBitmapImage(string base64)
        {
            var bitmapImage = new BitmapImage();
            var bytes = Convert.FromBase64String(base64);
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(bytes);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public static bool TryConvertBase64ToBitmapImage(this string base64, out BitmapImage bitmapImage)
        {
            try
            {
                bitmapImage = ConvertBase64ToBitmapImage(base64);

                return true;
            }
            catch (Exception)
            {
                bitmapImage = new BitmapImage();

                return false;
            }
        }

        public static bool TryConvertBitmapImageToBase64(this BitmapImage bitmapImage, out string base64)
        {
            base64 = string.Empty;

            try
            {
                using var memoryStream = new MemoryStream();
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(memoryStream);
                base64 = Convert.ToBase64String(memoryStream.ToArray());

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

            if (width < 0 || height < 0)
            {
                return false;
            }

            try
            {
                var uri = new Uri(path, UriKind.Absolute);
                bitmapImage.BeginInit();
                bitmapImage.UriSource = uri;
                bitmapImage.DecodePixelHeight = width;
                bitmapImage.DecodePixelWidth = height;
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
