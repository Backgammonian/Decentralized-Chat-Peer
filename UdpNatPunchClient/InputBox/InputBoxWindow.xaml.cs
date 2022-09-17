using System;
using System.Windows;

namespace InputBox
{
    public sealed partial class InputBoxWindow : Window
    {
        public InputBoxWindow()
        {
            InitializeComponent();
        }

        public string TitleText
        {
            get => Title;
            set => Title = value;
        }

        public string QuestionText
        {
            get => Question.Text;
            set => Question.Text = value;
        }

        public string AnswerText
        {
            get => Answer.Text;
            set => Answer.Text = value;
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
            Answer.SelectAll();
            Answer.Focus();
        }
    }
}