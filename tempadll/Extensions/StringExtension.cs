using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CoffeeJelly.tempadll.Extensions
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
            if (string.IsNullOrEmpty(str) || str.Last() == '\\')
                return str;
            return str + "\\";
        }

        public static string PathRemoveLastPart(this string str, out string lastPart)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (str == string.Empty)
                throw new ArgumentException($"{nameof(str)} should not be empty.");

            str.PathFormatter();
            var splittedStr = str.Split('\\');
            string formattedStr = string.Empty;
            for (int i = 0; i <= splittedStr.Length - 2; i++)
                formattedStr += splittedStr[i].PathFormatter();

            lastPart = splittedStr[splittedStr.Length - 1];
            return formattedStr;
        }

        public static bool EqualsAny(this string str, params string[] patterns)
        {
            foreach (string pattern in patterns)
                if (str.Equals(pattern))
                    return true;
            return false;
        }

        public static bool EqualsAny(this string str, StringComparison comparisonType, params string[] patterns)
        {
            foreach (string pattern in patterns)
                if (str.Equals(pattern, comparisonType))
                    return true;
            return false;
        }

        public static bool EqualsAny<T>(this T subj, params T[] patterns)
        {
            foreach (var pattern in patterns)
                if (subj.Equals(pattern))
                    return true;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pattern"></param>
        /// <param name="value"></param>
        /// <remarks>Заместить первый <paramref name="pattern"/> на <paramref name="value"/>.</remarks>
        /// <returns></returns>
        public static string ReplaceFirst(this string text, string pattern, string value)
        {
            var regex = new Regex(Regex.Escape(pattern));
            return regex.Replace(text, value, 1, 0);
        }

        public static int ParseNumber(this string str)
        {
            var regex = new Regex(@"-?\d+");
            var match = regex.Match(str);

            return match.Success ? Int32.Parse(match.Value) : 0;

        }

        public static int? TryParseNumber(this string str)
        {
            var regex = new Regex(@"-?\d+");
            var match = regex.Match(str);
            if (match.Success)
                return Int32.Parse(match.Value);
            return null;
        }

        public static int[] ParseNumbers(this string str)
        {
            return Regex.Matches(str, @"-?\d+").OfType<Match‌>().Select(m => Int32.Parse(m.Value)).ToArray();
        }

        public static string[] ParseNumbersAsStrings(this string str)
        {
            return Regex.Matches(str, @"-?\d+").OfType<Match‌>().Select(m => m.Value).ToArray();
        }

        //public static string ParseGrainbarError(this string str)
        //{
        //    var regex = new Regex(@"-?\Ed+");
        //    var match = regex.Match(str);
        //    if(match.Success)
        //        return regex.Replace(str, 

        //}

        public static string[] ParseDoubleNumbersAsStrings(this string str)
        {
            return Regex.Matches(str, @"-?\d+\.\d+").OfType<Match‌>().Select(m => m.Value).ToArray();
        }

    }
}
