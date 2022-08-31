using System;
using System.Windows;
using System.Windows.Controls;

namespace MyBehaviours
{
    public static class MediaElementBehaviours
    {
        public static readonly DependencyProperty IsAnimatedProperty =
            DependencyProperty.RegisterAttached("IsAnimated", typeof(bool), typeof(MediaElementBehaviours), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

        public static bool GetIsAnimated(DependencyObject obj) => (bool)obj.GetValue(IsAnimatedProperty);
        public static void SetIsAnimated(DependencyObject obj, bool value) => obj.SetValue(IsAnimatedProperty, value);

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mediaElement = d as MediaElement;
            if (mediaElement == null)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                mediaElement.UnloadedBehavior = MediaState.Manual;
                mediaElement.MediaEnded += OnMediaEnded;
            }
            else
            {
                mediaElement.UnloadedBehavior = MediaState.Stop;
                mediaElement.MediaEnded -= OnMediaEnded;
            }
        }

        private static void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            var mediaElement = sender as MediaElement;
            if (mediaElement == null)
            {
                return;
            }

            mediaElement.Position = new TimeSpan(0, 0, 1);
            mediaElement.Play();
        }
    }
}
