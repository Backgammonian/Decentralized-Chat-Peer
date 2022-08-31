using System.Windows;
using System.Windows.Input;
using UdpNatPunchClient.Models;
using MyBehaviours;

namespace ImageShowcase
{
    public partial class ImageShowcaseWindow : Window
    {
        public ImageShowcaseWindow(ImageItem imageItem)
        {
            InitializeComponent();

            if (imageItem.IsLoaded)
            {
                if (imageItem.IsAnimation)
                {
                    MediaElementBehaviours.SetIsAnimated(_displayImage, true);
                }

                _displayImage.Source = imageItem.PictureUri;
            }
            else
            {
                ShowImageErrorMessage(imageItem.PicturePath);
            }
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
