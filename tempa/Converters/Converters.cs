﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.Windows.Documents;

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
            return $"Масштаб: {((double)value).ToString("0.0", CultureInfo.InvariantCulture)}%";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class LogTextBlockConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!string.IsNullOrEmpty(value as string))
            {
                FlowDocument fd = new FlowDocument();

                string[] text = ((string)value).Split(' ');

                Paragraph p = new Paragraph();
                StringBuilder sb = new StringBuilder();

                //add text and pictures, etc. and return now InlineCollection instead of FlowDocument

                return p.Inlines;
            }
            else
            {
                return new FlowDocument();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
