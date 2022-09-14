using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using UdpNatPunchClient.Models;

namespace UdpNatPunchClient
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MessageTextBox.Focusable = true;

            var viewModel = (MainWindowViewModel)DataContext;
            viewModel.PassScrollingDelegate(ScrollMessageTextBoxToEnd);
            viewModel.PassMessageTextBoxFocusDelegate(FocusOnMessageTextBox);
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
