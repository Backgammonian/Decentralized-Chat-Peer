using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;

namespace Extensions
{
    public static class ListBoxExtensions
    {
        public static readonly DependencyProperty AutoScrollToEndProperty =
            DependencyProperty.RegisterAttached("AutoScrollToEnd", typeof(bool), typeof(ListBoxExtensions), new UIPropertyMetadata(OnAutoScrollToEndChanged));
        private static readonly DependencyProperty AutoScrollToEndHandlerProperty =
            DependencyProperty.RegisterAttached("AutoScrollToEndHandler", typeof(NotifyCollectionChangedEventHandler), typeof(ListBoxExtensions));

        public static bool GetAutoScrollToEnd(DependencyObject obj) => (bool)obj.GetValue(AutoScrollToEndProperty);
        public static void SetAutoScrollToEnd(DependencyObject obj, bool value) => obj.SetValue(AutoScrollToEndProperty, value);

        private static void OnAutoScrollToEndChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var listBox = s as ListBox;

            if (listBox == null)
            {
                return;
            }

            var source = (INotifyCollectionChanged)listBox.Items.SourceCollection;

            if ((bool)e.NewValue)
            {
                NotifyCollectionChangedEventHandler scrollToEndHandler = delegate
                {
                    if (listBox.Items.Count <= 0)
                    {
                        return;
                    }

                    listBox.Items.MoveCurrentToLast();
                    listBox.ScrollIntoView(listBox.Items.CurrentItem);
                };

                source.CollectionChanged += scrollToEndHandler;

                listBox.SetValue(AutoScrollToEndHandlerProperty, scrollToEndHandler);
            }
            else
            {
                var handler = (NotifyCollectionChangedEventHandler)listBox.GetValue(AutoScrollToEndHandlerProperty);

                source.CollectionChanged -= handler;
            }
        }
    }
}
