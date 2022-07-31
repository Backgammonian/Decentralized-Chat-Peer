using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using UdpNatPunchClient.Models;

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

        private void OnConnectedPeersListBoxLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            var userModel = listBoxItem.DataContext as UserModel;
            if (userModel == null)
            {
                return;
            }

            ((MainWindowViewModel)DataContext).SetSelectedUserModel(userModel);
        }
    }
}
