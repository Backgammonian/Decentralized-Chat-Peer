using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
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

            if (imageItem.IsAnimation)
            {
                try
                {
                    AnimationBehavior.SetSourceStream(DisplayImage, new FileStream(imageItem.Path, FileMode.Open, FileAccess.Read, FileShare.Read));
                }
                catch (Exception)
                {
                    ShowImageErrorMessage(imageItem.Path);
                }
            }
            else
            {
                if (BitmapImageExtensions.TryLoadBitmapImageFromPath(imageItem.Path, out var image))
                {
                    DisplayImage.Source = image;
                }
                else
                {
                    ShowImageErrorMessage(imageItem.Path);
                }
            }
        }

        private void ShowImageErrorMessage(string imagePath)
        {
            var imageName = Path.GetFileName(imagePath);
            ErrorTextBlock.Text = "Can't load image: " + imageName;
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
