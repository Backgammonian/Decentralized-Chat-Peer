using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;
using Extensions;
using UdpNatPunchClient.Models;

namespace ImageShowcase
{
    public partial class ImageShowcaseWindow : Window
    {
        public ImageShowcaseWindow(ImageItem imageItem)
        {
            InitializeComponent();

            if (imageItem.IsLoaded &&
                imageItem.IsAnimation)
            {
                AnimationBehavior.SetSourceStream(DisplayImage, imageItem.PictureStream);
            }
            else
            if (imageItem.IsLoaded &&
                BitmapImageExtensions.TryLoadBitmapImageFromPath(imageItem.PicturePath, out var image))
            {
                DisplayImage.Source = image;
            }
            else
            {
                ShowImageErrorMessage(imageItem.PicturePath);
            }
        }

        public ImageShowcaseWindow(BitmapImage image)
        {
            InitializeComponent();

            DisplayImage.Source = image;
        }

        private void ShowImageErrorMessage(string imagePath)
        {
            ErrorTextBlock.Text = $"Can't load image: {imagePath}";
            ErrorTextBlock.Visibility = Visibility.Visible;
            ImageBorder.Visibility = Visibility.Hidden;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            DialogResult = true;
        }
    }
}
