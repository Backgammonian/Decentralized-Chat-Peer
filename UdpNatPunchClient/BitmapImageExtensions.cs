using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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
                var encoder = new PngBitmapEncoder();
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

        public static bool TryConvertBitmapToBitmapImage(this Bitmap bitmap, out BitmapImage result)
        {
            result = new BitmapImage();

            try
            {
                using MemoryStream memory = new MemoryStream();
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                result.BeginInit();
                result.StreamSource = memory;
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.EndInit();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static BitmapImage ResizeImageWithPreservedAspectRatio(this Bitmap image, int width, int height)
        {
            var sourceWidth = image.Width;
            var sourceHeight = image.Height;
            var sourceX = 0;
            var sourceY = 0;
            var destX = 0;
            var destY = 0;
            var nPercentW = width / (float)sourceWidth;
            var nPercentH = height / (float)sourceHeight;

            float nPercent;
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = Convert.ToInt32((width - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = Convert.ToInt32((height - (sourceHeight * nPercent)) / 2);
            }

            var destWidth = (int)(sourceWidth * nPercent);
            var destHeight = (int)(sourceHeight * nPercent);

            using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            
            using var gr = Graphics.FromImage(bitmap);
            gr.Clear(Color.FromArgb(0, 0, 0, 0));
            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gr.DrawImage(image, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);

            return bitmap.TryConvertBitmapToBitmapImage(out var result) ? result : new BitmapImage();
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
