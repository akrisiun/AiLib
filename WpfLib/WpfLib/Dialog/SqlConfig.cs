using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Ai.Util
{
    public class SqlConfig
    {
        public SqlConfig()
        {
            ExceptionLog = null;
            Instance = this;
            lastExTrace = string.Empty;
            SqlCommandTimeout = 5;      // 5 seconds default;
        }

        static SqlConfig()
        {
            if (Instance == null)
                Instance = new SqlConfig();
        }

        public static SqlConfig Instance { get; protected set; }

        #region Settings

        public string sqlServer;
        public string sqlDatabase;
        public string sqlUsername;
        public string sqlPassword;
        public SqlConnection SqlConnection { get; private set; }

        public virtual void LoadSettings()
        {
            try
            {
                sqlServer = "";
                sqlServer = ConfigurationManager.AppSettings.Get("sql.server") ?? string.Empty;
                if (sqlServer.Length > 0)
                    ReadSqlSettings();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.Write("SqlConfig: Cannot read user properties  \r\n" + ex.Message);
            }
        }

        public virtual void ReadSqlSettings()
        {
            sqlDatabase = ConfigurationManager.AppSettings.Get("sql.database") ?? string.Empty;
            sqlUsername = ConfigurationManager.AppSettings.Get("sql.username") ?? string.Empty;
            sqlPassword = ConfigurationManager.AppSettings.Get("sql.password") ?? string.Empty;
            SqlCommandTimeout = GetIntValue("sql.commandTimeout", 10);  // 10 secs
        }

        #endregion

        #region AppSettings

        public virtual string GetStringValue(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings.Get(key) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public virtual int GetIntValue(string key, int def)
        {
            try
            {
                var str = ConfigurationManager.AppSettings.Get(key);
                int val = 0;
                if (Int32.TryParse(str, out val))
                    return val;
            }
            catch (Exception)
            {
            }
            return def;
        }

        #endregion

        #region Sql Connection

        public int SqlCommandTimeout  {get; set; }               // timeout in seconds

        public string SqlConnectionString
        {
            get
            {
                string s = "Database=" + sqlDatabase + ";Server=" + sqlServer
                   + ";Persist Security Info=False;MultipleActiveResultSets=True";
                s += ";User ID=" + sqlUsername + ";Pwd=" + sqlPassword;
                s += ";Connection Timeout=" + SqlCommandTimeout.ToString();     // timeout in seconds

                return s;
            }
        }

        public virtual bool IsOpenConnection()
        {
            if (SqlConnection != null && SqlConnection.State == System.Data.ConnectionState.Open)
                return true;
            return false;
        }

        public virtual bool OpenConnection()
        {
            if (SqlConnection != null && SqlConnection.State == System.Data.ConnectionState.Open)
                return true;

            SqlConnection = null;
            try
            {
                SqlConnection = new SqlConnection(SqlConnectionString);
                SqlConnection.Open();
                return true;
            }
            catch (Exception ex)
            {
                Dialog.ShowError("Cannot connect to SQL server\n\n" + ex.Message, false);
                return false;
            }
        }

        #endregion

        #region Fill Data

        public string[] LastError { get; private set; }
        public void SetLastError(string line1, string line2)
        {
            LastError = null;
            LastError = new string[2] { line1, line2 };
        }

        #endregion

        #region Exception log

        public event ExceptionLogHandler ExceptionLog;
        protected string lastExTrace;

        public void OnExceptionLog(object sender, string msg, Exception ex, bool reconnect)
        {
            if (ex != null && lastExTrace.Equals(ex.StackTrace))
                return;

            if (ExceptionLog != null)
            {
                var args = new ExceptionEventArgs(ex, msg);
                args.Reconnect = reconnect;
                ExceptionLog(sender, args);
            }
        }

        #endregion

    }

    public delegate void ExceptionLogHandler(object sender, ExceptionEventArgs e);

    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(Exception ex, string msg)
        {
            Exception = ex;
            Handled = false;
            Message = msg ?? string.Empty;
            Reconnect = false;
        }

        public Exception Exception { get; set; }
        public bool Handled;
        public string Message;
        public bool Reconnect;
    }

}
