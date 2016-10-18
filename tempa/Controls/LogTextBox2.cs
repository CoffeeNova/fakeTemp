//http://itknowledgeexchange.techtarget.com/wpf/textbox-with-dynamic-inlines/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace CoffeeJelly.tempa.Controls
{
    public class RichTextBlock : System.Windows.Controls.TextBlock
    {
        public static DependencyProperty InlineProperty;

        static RichTextBlock()
        {
            //OverrideMetadata call tells the system that this element wants to provide a style that is different than in base class
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RichTextBlock), new FrameworkPropertyMetadata(
                                typeof(RichTextBlock)));
            InlineProperty = DependencyProperty.Register("RichText", typeof(ObservableCollection<Inline>), typeof(RichTextBlock),
                            new PropertyMetadata(null, new PropertyChangedCallback(OnInlineChanged)));
        }
        public ObservableCollection<Inline> RichText
        {
            get { return (ObservableCollection<Inline>)GetValue(InlineProperty); }
            set { SetValue(InlineProperty, value); }
        }
        public static void OnInlineChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;
            RichTextBlock r = sender as RichTextBlock;
            ObservableCollection<Inline> i = e.NewValue as ObservableCollection<Inline>;
            if (r == null || i == null)
                return;
            r.Inlines.Clear();
            foreach (Inline inline in i)
            {
                r.Inlines.Add(inline);
            }
        }
    }
}
