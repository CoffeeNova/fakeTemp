using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace tempa.Extensions
{
    public static class AutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, AutoScrollPropertyChanged));


        public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var scrollViewer = obj as ScrollViewer;
            if (scrollViewer != null && (bool)args.NewValue)
            {
                scrollViewer.SizeChanged  += ScrollViewer_SizeChanged;
                scrollViewer.ScrollToEnd();
            }
            else
            {
                scrollViewer.LayoutUpdated -= ScrollViewer_SizeChanged;
            }
        }

        private static void ScrollViewer_SizeChanged(object sender, EventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
                scrollViewer.ScrollToEnd();
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
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(TextBoxAutoScrollBehavior), new PropertyMetadata(false, AutoScrollPropertyChanged));

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

}
