using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
// using System.Data.Linq;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

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

    // System.Data.Linq.DataContext
    public class Context : DataContext
    {
        #region Default Connection, Static Open
        public const string DefaultServer = @".\SQLEXPRESS"; // EnumValue.Get(Server.Local);
  
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
           Empty = new Context(conn: null);
        }
        #endregion

        public Context(string fileOrServerOrConnection)
            : base(null)
        {
            // #if !DATALINQ
            if (fileOrServerOrConnection != null && fileOrServerOrConnection.StartsWith("name="))
            {
                var name = fileOrServerOrConnection.Substring("name=".Length);
                var connStr = ConfigurationManager.ConnectionStrings[name].ConnectionString;
                base.Connection = new SqlConnection(connStr);
            }
            else
                base.Connection = new SqlConnection();

            dbName = base.Connection == null ? null : base.Connection.Database;
        }

        public Context(IDbConnection conn)
            : base(conn)
        {
            dbName = base.Connection == null ? null : base.Connection.Database;
            if (Instance == null)
                Instance = this;
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
        public virtual string ConnectionString() { return base.Connection == null ? null : base.Connection.ConnectionString; }
        #endregion

        #region Sql Execute methods
        public Exception LastError { get; set; }

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

        public T ExecuteScalar<T>(string sql)
        {
            SqlCommand cmd = new SqlCommand() { Connection = this.SqlConnection, CommandText = sql };

            Trace.WriteLine("ExecuteScalar: " + cmd.CommandText + " as " + typeof(T).Name);
            AssureOpen();
            return (T)cmd.ExecuteScalar();
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

                // list nodes : reader.NameTable;
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

        #endregion
    }

    public class SqlConnEventArgs : EventArgs
    {
        public SqlConnEventArgs(SqlConnection conn) { this.SqlConnection = conn; }

        public SqlConnection SqlConnection { get; private set; }
    }
}
