using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace CoffeeJelly.tempa.Converters
{
    public class SicButtonsMarginConverter : IValueConverter
    {

        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(System.Convert.ToDouble(value) - 55, -20, 0, 0);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class StringToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (value.GetType() == typeof(string)))
            {
                return ((string)value).ToUpper();
            }
            return value;
        }
        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public sealed class ValueToMethodConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ValueToMethodConverter can only be used for one way to source conversion.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var methodName = parameter as string;
            if (value == null || methodName == null)
                return value;
            var methodInfo = value.GetType().GetMethod(methodName, new Type[0]);
            if (methodInfo == null)
                return value;
            return methodInfo.Invoke(value, new object[0]);
        }
    }

    public class ScaleSliderTooltipConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return $"Масштаб {(value as string)}%";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }





}
