using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace Ai.Wpf
{
}

namespace Ai
{
    public static class Log
    {
        static Log()
        {
            // Assert.IsTrue(ConfigurationManager.AppSettings.Get("logdir") != null,
            //        new [] {@".config AppSettings\logdir error"});

            LogName = System.AppDomain.CurrentDomain.FriendlyName + ".log";
        }

        public static string LogName { get; set; }

        static FileStream StreamLog
        {
            get
            {
                if (LogName == null)
                    return null;
                string cFileName = ConfigurationManager.AppSettings.Get("logdir") + "\\" + LogName;
                if (cFileName == null)
                    return null;

                // Create the file and open it
                FileStream oFs;
                if (System.IO.File.Exists(cFileName))
                {
                    oFs = new FileStream(cFileName, FileMode.Append, FileAccess.Write);
                }
                else
                {
                    oFs = new FileStream(cFileName, FileMode.CreateNew, FileAccess.ReadWrite);
                }
                return oFs;
            }
        }

        public static StreamWriter Writer { get { return new StreamWriter(StreamLog); } }

        public static void Close() { StreamLog.Close(); }

        public static void Write(string cExpression)
        {
            if (LogName == null)
            {
                Trace.Write(cExpression);
                return;
            }
            try
            {
                //Create a writer for the file
                using (StreamWriter oWriter = Writer)
                {
                    // Write the contents
                    oWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                + " : " + cExpression);
                }
                Close();
            }
            catch { } // Ignore log file error
        }

        public static void WriteException(string cExpression, Exception ex)
        {
            if (LogName == null)
            {
                Trace.Write(cExpression + ex.Message);
                return;
            }
            try
            {
                using (StreamWriter oWriter = Writer)
                {
                    // Write the contents
                    oWriter.WriteLine(string.Format("{0} : {1}",
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            cExpression));
                    oWriter.WriteLine(ex.StackTrace);
                }
                Close();
            }
            catch { } // Ignore log file error
        }

    }

}