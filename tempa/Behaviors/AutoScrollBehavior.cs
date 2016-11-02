using System.Windows;
using System.Windows.Controls;

namespace CoffeeJelly.tempa.Behaviors
{
    public static class AutoScrollBehavior
    {
        private static bool _autoScroll = true;

        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior),
                new PropertyMetadata(false, AutoScrollPropertyChanged));


        public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var scrollViewer = obj as ScrollViewer;
            if (scrollViewer == null)
                return;
            if ((bool)args.NewValue)
                scrollViewer.ScrollChanged += scrollViewer_ScrollChanged;
            else
                scrollViewer.ScrollChanged -= scrollViewer_ScrollChanged;
        }



        static void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (e.ExtentHeightChange == 0)
            {
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                    _autoScroll = true;
                else
                    _autoScroll = false;
            }
            if (_autoScroll && e.ExtentHeightChange != 0)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
        }

        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }
    }

    public static class TextBoxAutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(TextBoxAutoScrollBehavior),
                new PropertyMetadata(false, AutoScrollPropertyChanged));

        public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var textBox = obj as TextBox;
            if (textBox != null && (bool)args.NewValue)
            {
                textBox.TextChanged += TextBox_TextChanged;
                textBox.CaretIndex = textBox.Text.Length;
                var rect = textBox.GetRectFromCharacterIndex(textBox.CaretIndex);
                textBox.ScrollToHorizontalOffset(rect.Right);
            }
            else
            {
                textBox.TextChanged -= TextBox_TextChanged;
            }
        }

        static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.CaretIndex = textBox.Text.Length;
                var rect = textBox.GetRectFromCharacterIndex(textBox.CaretIndex);
                textBox.ScrollToHorizontalOffset(rect.Right);
            }
        }

        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }

    }

    public static class TreeViewAutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(TreeViewAutoScrollBehavior),
                new PropertyMetadata(false, AutoScrollPropertyChanged));

        public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var treeViewItem = obj as TreeViewItem;
            if (treeViewItem != null && (bool)args.NewValue)
                treeViewItem.Expanded += TreeViewItem_Expanded;
            else
                treeViewItem.Expanded -= TreeViewItem_Expanded;
        }

        private static void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            var treeViewItem = e.OriginalSource as TreeViewItem;
            if (treeViewItem == null) return;

            var treeView = Internal.FindVisualParentElement(treeViewItem, typeof(TreeView));

            ScrollViewer scroller =
                (ScrollViewer)Internal.FindVisualChildElement(treeView, typeof(ScrollViewer));
            scroller.ScrollToBottom();
            treeViewItem.BringIntoView();
            e.Handled = true;

        }

        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }

    }

}
