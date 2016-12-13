using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeJelly
{
    internal static class InternalExtensions
    {
        internal static System.Windows.Rect ConvertToRect(this System.Drawing.Rectangle value)
        {
            return new System.Windows.Rect(value.X, value.Y, value.Width, value.Height);
        }
    }
}
