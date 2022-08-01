using System.Windows;
using System.Windows.Controls;

namespace PlaceholderTextBoxControl
{
    public class PlaceholderTextBox : TextBox
    {
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(PlaceholderTextBox),
                new PropertyMetadata(string.Empty));

        public bool IsEmpty
        {
            get => (bool)GetValue(IsEmptyProperty);
            set => SetValue(IsEmptyPropertyKey, value);

        }

        private static readonly DependencyPropertyKey IsEmptyPropertyKey =
            DependencyProperty.RegisterReadOnly("IsEmpty", typeof(bool), typeof(PlaceholderTextBox),
                new PropertyMetadata(true));

        public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            IsEmpty = string.IsNullOrEmpty(Text);

            base.OnTextChanged(e);
        }

        static PlaceholderTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlaceholderTextBox), new FrameworkPropertyMetadata(typeof(PlaceholderTextBox)));
        }
    }
}