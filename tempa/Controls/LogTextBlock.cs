//stealed from https://social.msdn.microsoft.com/Forums/vstudio/en-US/82b30e02-aac4-4564-9e3b-05d5622b9005/how-can-i-bind-a-textblock-to-formatted-text?forum=wpf

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CoffeeJelly.tempa.Controls
{
    public class LogTextBlock : TextBlock
    {
        public InlineCollection InlineCollection
        {
            get
            {
                return (InlineCollection)GetValue(InlineCollectionProperty);
            }
            set
            {
                SetValue(InlineCollectionProperty, value);
            }
        }

        public static readonly DependencyProperty InlineCollectionProperty = DependencyProperty.Register(
            "InlineCollection",
            typeof(InlineCollection),
            typeof(LogTextBlock),
                new UIPropertyMetadata((PropertyChangedCallback)((sender, args) =>
                {
                    LogTextBlock textBlock = sender as LogTextBlock;

                    if (textBlock != null)
                    {
                        textBlock.Inlines.Clear();

                        InlineCollection inlines = args.NewValue as InlineCollection;

                        if (inlines != null)
                            textBlock.Inlines.AddRange(inlines.ToList());
                    }
                })));

    }
}
