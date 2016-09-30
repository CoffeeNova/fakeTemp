using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace tempa.Extensions
{
    public static class StringExtension
    {
        public static bool AnyNullOrEmpty(params string[] str)
        {
            return str.Any(s => string.IsNullOrEmpty(s));
        }

        public static string FormatToGrainBarDate(this string str)
        {
            return Regex.Replace(str, "<, *?>", string.Empty);
        }

        public static string RemoveWhiteSpaces(this string str)
        {
            return Regex.Replace(str, " ", string.Empty);
        }

        public static string[] RemoveWhiteSpaces(this string[] strArr)
        {
            foreach (var str in strArr)
                str.RemoveWhiteSpaces();
            return strArr;
        }

        public static string RemoveGrainBarErrorValue(this string str)
        {
            if (str.StartsWith("E"))
                return string.Empty;
            return str;
        }

        public static string PathFormatter(this string str)
        {
            if (str.Last() != '\\')
                return str + "\\";
            return str;
        }
    }
}
