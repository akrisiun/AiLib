using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using System.Linq;
using System.Configuration;          // in System.Core.dll

#if DATALINQ || WEB
using System.Data.Linq;
#else
namespace Ai.Entity
{
    public class DataContext
    {
        public DataContext(IDbConnection connection)
        {
            Connection = connection;
        }
        public int CommandTimeout { get; set; }
        public IDbConnection Connection { get; set; }
    }
}
#endif


namespace Ai.Entity
{
    // System.Data.Linq.DataContext
    public class Context : DataContext
    {
        #region Default Connection, Static Open
        //public const string DefaultServer = @".\SQLEXPRESS";
        //public const int defConnectTimeout = 5;
        //public const int defCommandTimeout = 10;

        public static Context OpenWithConnStr(string fileOrServerOrConnection)
        {
            if (Instance != null && Instance.Connection == null)
                Instance = null;
            Instance = Context.Instance ?? new Context(fileOrServerOrConnection);
            Instance.AssureOpen();
            if (Instance.dbName == null && Instance.Connection != null)
                Instance.dbName = Instance.Connection.Database;
            return Instance;
        }

        public static string UserID;
        public static string Password;
        public static bool? IntegratedSecurity;
        #endregion

        #region Static 
        public static Context Instance;
        public static Context Empty { get; protected set; }

        static Context()
        { 
           UserID = null;
           Password = null;
           IntegratedSecurity = null;
           Empty = new Context();
        }
        #endregion

        public Context()
            : base(new SqlConnection())
        { }

        public Context(string fileOrServerOrConnection)
#if DATALINQ
            : base(fileOrServerOrConnection)
#else
            : base(null)
#endif
        {
#if !DATALINQ
            if (fileOrServerOrConnection != null && fileOrServerOrConnection.StartsWith("name="))
            {
                var name = fileOrServerOrConnection.Substring("name=".Length);
                var connStr = ConfigurationManager.ConnectionStrings[name].ConnectionString;
                base.Connection = new SqlConnection(connStr);
                if (base.Connection != null)
                    base.Connection.Open();
            }
            else 
                base.Connection = new SqlConnection();
#endif

            dbName = base.Connection == null ? null : base.Connection.Database;
        }

        public Context(IDbConnection conn)
            : base(conn)
        {
            dbName = base.Connection == null ? null : base.Connection.Database;
            if (Instance == null)
                Instance = this;
        }

        public virtual string ConnectionString()
        {
            // return Context.ConnectionString(Context.DefaultServerEnum, Context.InitialCatalog);
            return base.Connection.ConnectionString;
        }

        #region Connection methods
        public EventHandler<SqlConnEventArgs> OnBeforeOpen { get; set; }
        public EventHandler<SqlConnEventArgs> OnAfterOpen { get; set; }

        private string dbName;
        public virtual string DbName { get { return dbName; } }
        public override string ToString()
        {
            return string.Format("{0},{1}", SqlConnection != null ? SqlConnection.DataSource : "-", DbName);
        }

        public virtual bool AssureOpen()
        {
            if (SqlConnection.State == ConnectionState.Open)
            {
                if (SqlConnection.Database != DbName)
                    SqlConnection.ChangeDatabase(DbName);
                return true;
            }
            if (this.OnBeforeOpen != null)
                this.OnBeforeOpen(this, new SqlConnEventArgs(SqlConnection));

            SqlConnection.Open();
            if (SqlConnection.Database != DbName)
                SqlConnection.ChangeDatabase(DbName);

            if (this.OnAfterOpen != null)
                this.OnAfterOpen(this, new SqlConnEventArgs(SqlConnection));
            return SqlConnection.State == ConnectionState.Open;
        }

        public virtual SqlConnection SqlConnection { get { return base.Connection as SqlConnection; } }
        #endregion

        #region Sql Execute methods
        public Exception LastError { get; set; }

        #endregion

        #region Execute Sql

        public IList<T> Exec<T>(SqlProc procedure) where T : class
        {
            if (procedure.Connection == null)
            {
                procedure.Connection = SqlConnection;
                if (procedure.Connection == null)
                    throw new ArgumentNullException("procedure.Connection");
            }

            Trace.WriteLine("SqlProc: " + procedure.CmdText);

            var list = new Collection<T>();
            return procedure.Exec<T>(list, null); // .Result;
        }

        public XDocument ExecXml(SqlProc procedure)
        {
            if (procedure.Connection == null)
                procedure.Connection = SqlConnection;

            AssureOpen();
            var cmd = procedure.CreateCommand();
            Trace.WriteLine("ExecXml " + cmd.CommandText);

            XDocument doc = null;
            using (var reader = cmd.ExecuteXmlReader())
            {
                XDocument docNext = null;

                while (reader.Read())
                {
                    if (doc == null)
                        doc = XDocument.Load(reader.ReadSubtree());
                    else
                    {
                        docNext = XDocument.Load(reader.ReadSubtree());
                        doc.Document.Root.Add(docNext.Root);
                        docNext = null;
                    }
                }
            }
            return doc;
        }

        public object ExecuteScallar(string cmdText, params object[] parameters)
        {
            var cmd = new SqlCommand(cmdText, this.SqlConnection);
            return cmd.ExecuteScalar();
        }

        public T ExecuteScallar<T>(string cmdText, params object[] parameters)
        {
            var cmd = new SqlCommand(cmdText, this.SqlConnection);
            return (T)cmd.ExecuteScalar();
        }

        public int ExecuteNonQuery(string cmdText, params object[] parameters)
        {
            var cmd = new SqlCommand(cmdText, this.SqlConnection);
            return cmd.ExecuteNonQuery();
        }

        #endregion
    }

    public class SqlConnEventArgs : EventArgs
    {
        public SqlConnEventArgs(SqlConnection conn) { this.SqlConnection = conn; }

        public SqlConnection SqlConnection { get; private set; }
    }
}
