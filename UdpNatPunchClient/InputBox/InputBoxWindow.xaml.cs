using System;
using System.Windows;

namespace InputBox
{
    public partial class InputBoxWindow : Window
    {
        public InputBoxWindow(string title, string question, string defaultAnswer = "")
        {
            InitializeComponent();

            Title = title;
            _question.Content = question;
            _answer.Text = defaultAnswer;
        }

        public string Answer => _answer.Text;

        private void OkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
            _answer.SelectAll();
            _answer.Focus();
        }
    }
}