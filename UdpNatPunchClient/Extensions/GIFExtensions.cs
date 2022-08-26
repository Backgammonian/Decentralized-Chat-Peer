using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Gif.Components;

namespace Extensions
{
    public static class GIFExtensions
    {
        public static async Task<(bool, List<Frame>)> TryGetResizedFramesFromGIFAsync(FileStream stream, int newWidth, int newHeight)
        {
            return await Task.Run(() => TryGetResizedFramesFromGIF(stream, newWidth, newHeight));
        }

        public static (bool, List<Frame>) TryGetResizedFramesFromGIF(FileStream stream, int newWidth, int newHeight)
        {
            var frames = new List<Frame>();

            try
            {
                using var image = (Bitmap)Image.FromStream(stream);
                var frameCount = image.GetFrameCount(FrameDimension.Time);

                if (frameCount == 0)
                {
                    return (false, frames);
                }

                //Get the times stored in the gif
                //PropertyTagFrameDelay ((PROPID) 0x5100) comes from gdiplusimaging.h
                //More info on http://msdn.microsoft.com/en-us/library/windows/desktop/ms534416(v=vs.85).aspx
                var propertyTagFrameDelay = image.GetPropertyItem(0x5100);

                byte[] times;
                if (propertyTagFrameDelay != null &&
                    propertyTagFrameDelay.Value != null)
                {
                    times = propertyTagFrameDelay.Value;
                }
                else
                {
                    times = new byte[4 * frameCount];
                    for (int i = 0; i < times.Length; i++)
                    {
                        times[i] = 10;
                    }
                }

                for (int i = 0; i < frameCount; i++)
                {
                    image.SelectActiveFrame(FrameDimension.Time, i);
                    var duration = BitConverter.ToInt32(times, 4 * i);
                    var resizedFrame = image.ResizeImageWithPreservedAspectRatio(newWidth, newHeight);
                    frames.Add(new Frame(resizedFrame, duration));
                }

                return (true, frames);
            }
            catch (Exception)
            {
                return (false, frames);
            }
        }

        public static async Task<bool> TryCreateGIFAsync(string gifPath, List<Frame> frames)
        {
            return await Task.Run(() => TryCreateGIF(gifPath, frames));
        }

        public static bool TryCreateGIF(string gifPath, List<Frame> frames)
        {
            try
            {
                var encoder = new AnimatedGifEncoder();
                encoder.Start(gifPath);
                encoder.SetRepeat(0);

                foreach (var frame in frames)
                {
                    encoder.AddFrame(frame.Image, frame.Duration);
                }

                encoder.Finish();

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}