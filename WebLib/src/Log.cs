using System;
using System.Configuration;
using System.IO;

namespace Ai.Web
{

    public static class Log
    {
        static Log()
        {
            Ai.Assert.IsTrue(ConfigurationManager.AppSettings.Get("logdir") != null,
                    new [] {@".config AppSettings\logdir error"});
        }

        static FileStream StreamLog
        {
            get
            {
                string cFileName = ConfigurationManager.AppSettings.Get("logdir") + "\\prekes.log";
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

        public static void Write(string cExpression)
        {
            try
            {
                var oFs = StreamLog;

                //Create a writer for the file
                using (StreamWriter oWriter = new StreamWriter(oFs))
                {
                    // Write the contents
                    oWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                + " : " + cExpression);
                }
                oFs.Close();
            }
            catch  // (Exception exp )
            {      // Klaida atidarant klaidu faila...
            }
        }

        public static void WriteException(string cExpression, Exception ex)
        {
            try
            {
                var oFs = StreamLog;
                //Create a writer for the file
                using (StreamWriter oWriter = new StreamWriter(oFs))
                {
                    // Write the contents
                    oWriter.WriteLine(string.Format("{0} : {1}",
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            cExpression));
                    oWriter.WriteLine(ex.StackTrace);
                }
                oFs.Close();
            }
            catch  // (Exception exp )
            {      // Klaida atidarant klaidu faila...
            }
        }

    }

}