using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ai.XHtml
{
    public static class StringExt
    {
        public static string StrExtract(this string str, string from, string till = "", string fromEnd = "")
        { 
            int pos1 = str.IndexOf(from);
            if (pos1 < 0) return string.Empty;

            string strRest = str.Substring(pos1 + from.Length);
            if (fromEnd.Length > 0)
            {
                pos1 = strRest.IndexOf(fromEnd);
                if (pos1 < 0)
                    return string.Empty;

                strRest = strRest.Substring(pos1 + fromEnd.Length);
            }

            int pos2 = strRest.IndexOf(till);
            if (pos2 < 0) return strRest;

            return strRest.Substring(0, pos2);
        }

        public static string ProperInvariant(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            return char.ToUpperInvariant(str[0]) + str.Substring(1).ToLower();
        }

        public static string Proper(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            return char.ToUpper(str[0], System.Globalization.CultureInfo.CurrentCulture)
                 + str.Substring(1).ToLower();
        }

    }
}
