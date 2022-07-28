using System.Windows;
using System.ComponentModel;

namespace UdpNatPunchClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).PassScrollingDelegate(ScrollMessageTextBoxToEnd);
            ((MainWindowViewModel)DataContext).StartApp();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ((MainWindowViewModel)DataContext).ShutdownApp();
        }

        private void ScrollMessageTextBoxToEnd()
        {
            MessageTextBox.ScrollToEnd();
        }
    }
}
