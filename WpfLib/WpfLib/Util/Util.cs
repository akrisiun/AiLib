using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Ai.Util
{
    public static class Util
    {

        public static void ErrorDialogShow(Exception err, string caption)
        {
            DialogError.Show(err, caption);
        }

        public static string ExeVersionString
        {
            get
            {
                System.Version ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                string sver = string.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
                return sver;
            }
        }

        public static string ReplaceInvalidChars(string s)
        {
            s = s.Replace('\x00', '?');
            s = s.Replace('\x02', '?');
            s = s.Replace('\r', '?');
            s = s.Replace('\n', '?');

            return s;
        }

        public static string Left(string text, int len)
        {
            if (text.Length > len)
                text = text.Substring(0, len);
            return text;
        }


        public static int Parse(string num)
        {
            return Int32.Parse(num);
        }
    }

    public class UtilApplicationException : ApplicationException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strErrDescription"></param>
        public UtilApplicationException(string strErrDescription) :
            base(strErrDescription) { }

    }

}
