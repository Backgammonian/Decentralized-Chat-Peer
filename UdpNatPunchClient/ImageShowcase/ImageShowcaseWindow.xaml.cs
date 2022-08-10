using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ImageShowcase
{
    public partial class ImageShowcaseWindow : Window
    {
        public ImageShowcaseWindow(BitmapImage image)
        {
            InitializeComponent();

            DisplayImage.Source = image;
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
