using System.Drawing;

namespace Extensions
{
    public class Frame
    {
        public Frame(Bitmap image, int duration)
        {
            Image = image;
            Duration = duration;
        }

        public Bitmap Image { get; }
        public int Duration { get; }
    }
}
