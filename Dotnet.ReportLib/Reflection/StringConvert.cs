using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dotnet // .Reflection
{
    public static class StringConvert
    {

        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }


        // Web: StringEq.SubStringSafe
        public static string SubStrSafe(this string str, int pos1, int length = 0)
        {
            if (str == null || str.Length == 0 || pos1 > str.Length)
                return str;

            if (pos1 + length > str.Length)
                return str.Substring(pos1);

            return str.Substring(pos1, length);
        }


        public static string ToStringIfNull(this object obj, string ifNull = "")
        {
            if (obj == null)
                return ifNull;
            if (obj is String)
                return obj as string ?? ifNull;

            if (obj is XAttribute)
                return (obj as XAttribute).Value ?? ifNull;
            else if (obj is XElement)
                return (obj as XElement).Value ?? ifNull;
            else if (obj is IEnumerable<object>)
                return JoinStr<object>(obj as IEnumerable<object>);
            if (obj is IEnumerable)
                return JoinStr(obj as IEnumerable);

            return obj.ToString() ?? ifNull;
        }


        public static string StrLeft(this string str, int left)
        {
            if (str == null || str.Length < left)
                return str ?? String.Empty;
            return str.Substring(0, left);
        }

        public static string StrLeft(object obj, int left)
        {
            if (obj == null)
                return String.Empty;
            var str = obj is string ? obj as string : obj.ToString();
            if (str.Length < left)
                return str ?? String.Empty;
            return str.Substring(0, left);
        }

        public static bool ContainsCase(this string str, string substring,
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return str.IndexOf(substring, comparisonType: comparisonType) >= 0;
        }

        public static bool EqualsCase(this string str, object obj,
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            if (str == null && obj == null)
                return false;

            var objStr = obj is string ? obj as string : obj.ToString();
            return String.Equals(str, objStr, comparisonType: comparisonType);
        }

        public static bool StrEquals<T>(this T objA, T objB,
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) where T : class
        {
            if (objA == null || objB == null)
                return false;
            if (Object.ReferenceEquals(objA, objB))
                return true;

            var strA = objA.ToString();
            var strB = objB.ToString();
            if (string.IsNullOrWhiteSpace(strA) && string.IsNullOrWhiteSpace(strB))
                return true;

            return string.Equals(strA, strB, comparisonType: comparisonType);
        }

        public static bool IsArrayEmpty(this object[] data)
        {
            if (data == null || data.Length == 0)
                return true;
            int index = 0;
            while (index < data.Length)
            {
                if (data[index] == null || string.IsNullOrWhiteSpace(data[index].ToString()))
                    return true;
                index++;
            }

            return false;
        }

        public static string[] NewLines = new string[] { System.Environment.NewLine };
        public static char[] NewChar = new char[] { '\n' };
        public static string NewCharRemove = "\r";

        // Safe null
        public static string[] SplitNewLines(this string str)
        {
            return str == null ? null : str.Split(NewLines, options: StringSplitOptions.RemoveEmptyEntries);
        }

        public static string JoinStr(this IEnumerable<string> str)
        {
            if (str == null)
                return String.Empty;

            var build = new StringBuilder();
            var numer = str.GetEnumerator();
            while (numer.MoveNext())
                build.Append(numer.Current);

            return build.ToString();
        }

        public static string JoinStr<T>(this IEnumerable<T> str, string separator = " ")
        {
            if (str == null)
                return null;

            var buff = new StringBuilder();
            var num = str.GetEnumerator();
            while (num.MoveNext())
            {
                var item = ToStringIfNull(num.Current, string.Empty);
                if (item.Length > 0)
                {
                    if (buff.Length > 0 && separator.Length > 0)
                        buff.Append(separator);
                    buff.Append(item);
                }
            }

            return buff.ToString();
        }

        public static string JoinStr(this IEnumerable str, string separator = " ")
        {
            if (str == null)
                return null;

            var buff = new StringBuilder();
            var num = str.GetEnumerator();
            while (num.MoveNext())
            {
                var item = ToStringIfNull(num.Current, string.Empty);
                if (item.Length > 0)
                {
                    if (buff.Length > 0 && separator.Length > 0)
                        buff.Append(separator);
                    buff.Append(item);
                }
            }

            return buff.ToString();
        }

        public static string[] SplitLines(this string str)
        {
            if (str == null) return new string[1] { null };

            var fixStr = str.Replace(NewCharRemove, string.Empty);
            return fixStr.Split(NewChar, options: StringSplitOptions.None);
        }

        // Safe null
        public static string PadRight(this string str, int len, char paddingChar = ' ')
        {
            return str == null ? null : str.PadRight(len, paddingChar);
        }
    }
}
