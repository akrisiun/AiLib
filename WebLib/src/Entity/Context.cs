using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Linq;
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
    public enum Server
    {
        [EnumValue(@".\SQLEXPRESS")]
        Local = 0,
        [EnumValue(@"SNTXTX-VM0\SNTXSQL1")]
        VM0 = 1,
        [EnumValue(@"SNTXTX-VM0\SNTXSQL1")]
        VM0_ccUsrWeb = 2,
        [EnumValue(@"BLSTX-SQL1.bls.lt")]
        Bls = 3
    }

    // System.Data.Linq.DataContext
    public class Context : DataContext
    {
        #region Default Connection, Static Open
        public const Server DefaultServerEnum = Server.Local;
        public const string DefaultServer = @".\SQLEXPRESS"; // EnumValue.Get(Server.Local);
        public const bool DefaultIntegratedSecurity = false;
        public const string DefaultUser = "ccusrweb";
        public const string DefaultPass = "ccwebusr";
        public const string InitialCatalog = "SNTXDB";
        public const int defConnectTimeout = 5;
        public const int defCommandTimeout = 10;

        public static Context Open(Server serverEnum)
        {
            return Open(serverEnum, InitialCatalog, true);
        }

        public static Context Open(Server serverEnum, string database, bool openConnection)
        {
            Instance = Context.Instance ?? new Context();
            Instance.OpenConnection(serverEnum, database, openConnection);
            return Instance;
        }

        public bool OpenConnection(Server serverEnum, string database, bool openConnection)
        {
            string serverName = EnumValue.Get(serverEnum);
            var conn = this.Connection;
            if (serverEnum == Server.VM0_ccUsrWeb)
                conn.ConnectionString = new SqlConnectionStringBuilder()
                {
                    DataSource = EnumValue.Get(serverEnum),
                    InitialCatalog = database,
                    UserID = "ccusrweb",
                    Password = "ccwebusr",
                    IntegratedSecurity = DefaultIntegratedSecurity,
                    ConnectTimeout = defConnectTimeout
                }.ConnectionString
                 + ";Trusted_Connection=false;";
            else
                if (conn.State != ConnectionState.Open && string.IsNullOrEmpty(conn.ConnectionString))
                    conn.ConnectionString = ConnectionString(serverEnum, database);

            if (!conn.DataSource.Equals(serverName))
                throw new System.OperationCanceledException("connection server error");
            if (!conn.Database.Equals(database))
                throw new System.OperationCanceledException("connection database server error");

            if (openConnection && conn.State != ConnectionState.Open)
            {
                if (this.OnBeforeOpen != null)
                    this.OnBeforeOpen(this, new SqlConnEventArgs(conn as SqlConnection));
                conn.Open();
            }
            dbName = database;
            conn.ChangeDatabase(DbName);

            this.CommandTimeout = defCommandTimeout;
            return Connection.State == ConnectionState.Open;
        }

        public virtual string ConnectionString()
        {
            return Context.ConnectionString(Context.DefaultServerEnum, Context.InitialCatalog);
        }

        public static string ConnectionString(Server serverEnum, string database)
        {
            string serverName = EnumValue.Get(serverEnum);
            Instance.dbName = database;
            return new SqlConnectionStringBuilder()
                    {
                        DataSource = serverName,
                        InitialCatalog = Instance.dbName,
                        PersistSecurityInfo = true,
                        IntegratedSecurity = DefaultIntegratedSecurity,
                        UserID = DefaultUser,
                        Password = DefaultPass,
                        ConnectTimeout = defConnectTimeout
                    }.ConnectionString
                    + ";Trusted_Connection=false;";
        }
        #endregion

        public static Context Instance;
        public Context()
            : base(new SqlConnection())
        { }

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

        public T ExecScalar<T>(string sql)
        {
            SqlCommand cmd = new SqlCommand() { Connection = this.SqlConnection, CommandText = sql };

            Trace.WriteLine("ExecScalar: " + cmd.CommandText + " as " + typeof(T).Name);
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
