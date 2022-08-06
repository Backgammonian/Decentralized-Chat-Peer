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

            MessageTextBox.Focusable = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (MainWindowViewModel)DataContext;

            viewModel.PassScrollingDelegate(ScrollMessageTextBoxToEnd);
            viewModel.PassMessageTextBoxFocusDelegate(FocusOnMessageTextBox);
            viewModel.StartApp();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ((MainWindowViewModel)DataContext).ShutdownApp();
        }

        private void ScrollMessageTextBoxToEnd()
        {
            MessageTextBox.ScrollToEnd();
        }

        private void FocusOnMessageTextBox()
        {
            MessageTextBox.Focus();
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
