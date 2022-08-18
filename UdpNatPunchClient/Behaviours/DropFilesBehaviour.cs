using System.Windows;
using System.Windows.Input;

namespace DropFiles
{
    //source: https://stackoverflow.com/questions/5916154/how-to-handle-drag-drop-without-violating-mvvm-principals

    public class DropFilesBehavior
    {
        public static readonly DependencyProperty FilesDropCommandProperty =
            DependencyProperty.RegisterAttached("FilesDropCommand", typeof(ICommand), typeof(DropFilesBehavior), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropChanged)));

        private static void OnPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)d;

            fe.AllowDrop = true;
            fe.Drop += OnDrop;
            fe.PreviewDragOver += OnPreviewDragOver;
        }

        private static void OnPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            var element = (FrameworkElement)sender;

            var command = GetFilesDropCommand(element);

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
            {
                command.Execute(new FilesDroppedEventArgs(files));
            }
        }

        public static void SetFilesDropCommand(UIElement element, ICommand value)
        {
            element.SetValue(FilesDropCommandProperty, value);
        }

        public static ICommand GetFilesDropCommand(UIElement element)
        {
            return (ICommand)element.GetValue(FilesDropCommandProperty);
        }
    }
}
