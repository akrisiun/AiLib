using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml.Linq;

#if  !NET40 || WEB || WPF || NETSTANDARD20 || NETCOREAPP3_0
namespace System.Data.Linq
{
    public abstract class DataContext  : IDisposable
    {
        public DataContext(DbConnection connection = null)
        {
            Connection = connection;
        }

        public DataContext(object source) { Connection = source as DbConnection; }

        public int CommandTimeout { get; set; }
        public DbConnection Connection { get; set; }

        protected abstract void Dispose(bool disposing);
        public void Dispose() { this.Dispose(false); }
    }
}
#endif

namespace Dotnet.Entity
{
    using System.Data.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    // System.Data.Linq.DataContext

    [DebuggerDisplay("Context[ SPID={SPID}, Conn={Connection?.DataSource ?? \"-\"} ]")]
    public class Context : DataContext, ISqlContext
    {
        #region Default Connection, Static Open

        public static string InitialCatalog = "SNTXDB";
        public const int defConnectTimeout = 5;
        public const int defCommandTimeout = 10;
        public const bool DefaultIntegratedSecurity = false;
        public static string DefaultUser = "ccusrweb";
        public static string DefaultPass = "ccwebusr";

        #if !NETCORE30
        // #if WEB || NET45
        public static Context OpenWithConnKey(string appKey, string initialCatalog = null)
        {
            LastConnKey = appKey;

            var str = System.Configuration.ConfigurationManager
                .ConnectionStrings[appKey].ConnectionString;
            return OpenWithConnStr(str, initialCatalog);
        }

        public static Context NewWithConnKey(string appKey, string initialCatalog = null)
        {
            LastConnKey = appKey;
            var str = System.Configuration.ConfigurationManager
                .ConnectionStrings[appKey].ConnectionString;
            return NewWithConnStr(str, initialCatalog);
        }
        // #endif
#endif

        public static string LastConnKey { get; set; }
        public static string LastConnString { get; protected set; }
        public static void SetLastConnString(string str) { LastConnString = str ?? LastConnString; }

        public static Context OpenWithConnStr(string fileOrServerOrConnection, string initialCatalog = null)
        {
            var newConn = new SqlConnectionStringBuilder(fileOrServerOrConnection);
            if (initialCatalog != null)
                newConn.InitialCatalog = initialCatalog;

            var secondInst = new Context(newConn.ConnectionString);
            secondInst.LastError = null;
            if (!secondInst.AssureOpen(true)) {
                var sqlConnErr = secondInst.LastError as SqlException;
                if (sqlConnErr != null) {
                    throw sqlConnErr;       // fatal SQL connection / user auth / database error 
                }
                return null;    
            }
            secondInst.dbName = newConn.InitialCatalog;
            return secondInst;
        }

        public static Context NewWithConnStr(string fileOrServerOrConnection, string initialCatalog = null)
        {
            var newConn = new SqlConnectionStringBuilder(fileOrServerOrConnection);
            if (initialCatalog != null)
                newConn.InitialCatalog = initialCatalog;

            var secondInst = new Context(newConn.ConnectionString);
            secondInst.dbName = newConn.InitialCatalog;
            return secondInst;
        }


        public static string UserID;
        public static string Password;
        public static bool? IntegratedSecurity;
        #endregion

        #region Static

#if !NET45 && !DOTNET && !NETCORE && !WPF
        [Obsolete]
        public static Context Instance { get; set; }
#endif

        static Context()
        {
            UserID = null;
            Password = null;
        }

        #endregion

        #region ctor
        public Context() : this(new SqlConnection()) { }

        public Context(string fileOrServerOrConnection, string connKey = null)
            : base(fileOrServerOrConnection)
        {
            _isDisposed = false;
            ConnKey = connKey ?? LastConnKey;
            dbName = base.Connection == null ? null : base.Connection.Database;

            if (fileOrServerOrConnection is string) {
                LastConnString = fileOrServerOrConnection as string;
                if (!string.IsNullOrWhiteSpace(LastConnString))
                    this.Connection = new SqlConnection(LastConnString);
            }

        }

        public Context(IDbConnection conn, string connKey = null)
            : base(conn)
        {
            _isDisposed = false;
            ConnKey = connKey ?? LastConnKey;
            dbName = base.Connection == null ? null : base.Connection.Database;
        }

        #endregion

        public string ConnKey { get; set; }
        public Exception LastError { get; set; }

        public virtual string ConnectionString()
        {
            try {
#if !NETCORE30
                if (Connection == null && !String.IsNullOrWhiteSpace(LastConnKey))
                    return System.Configuration.ConfigurationManager.ConnectionStrings[LastConnKey].ConnectionString;
#endif
                if (LastConnString != null
                    && (IsDisposed || base.Connection == null || string.IsNullOrWhiteSpace(base.Connection.ConnectionString))
                    || Connection == null)
                    return LastConnString;

                if (!IsDisposed && base.Connection != null && !string.IsNullOrWhiteSpace(base.Connection.ConnectionString))
                    return base.Connection.ConnectionString;
            }
            catch { }

            if (this._proxy != null)
                return _proxy.ConnectionString; // base.Connection.ConnectionString;

            return null;
        }

        public virtual DbProviderFactory DbProviderFactory {
            get {
                return System.Data.SqlClient.SqlClientFactory.Instance;
                // sealed class EntityProviderFactory : DbProviderFactory, IServiceProvider
            }
        }

        #region Connection methods

        public bool IsDisposed { [DebuggerStepThrough] get => _isDisposed; }
        protected bool _isDisposed;

        protected override void Dispose(bool disposing)
        {
            _isDisposed = true;
            if (this._proxy != null) {
                this._proxy.Dispose();
                this._proxy = null;
            }
        }

        public EventHandler<SqlConnEventArgs> OnBeforeOpen { get; set; }
        public EventHandler<SqlConnEventArgs> OnAfterOpen { get; set; }

        private string dbName;
        public virtual string DbName { get { return dbName; } }
        public override string ToString()
        {
            var conn = SqlConnection;
            if (conn == null)
                return string.Format("-, {0}", DbName);

            try {
                var connParts = new SqlConnectionStringBuilder(conn.ConnectionString);
                return string.Format("{0}@{1},{2},SPID={3}", connParts.UserID, conn.DataSource, conn.Database, SPID ?? -1);
            }
            catch {
                return Connection?.DataSource ?? "-";
            }
        }

        public virtual bool ChangeDatabase(string db)
        {
            dbName = db;
            if (SqlConnection.State == ConnectionState.Open)
                SqlConnection.ChangeDatabase(db);
            return DbName.Equals(db);
        }

        public int? SPID { get; protected set; }

        public void UpdateSpid(int? spid = null)
        {
            if (spid.HasValue)
                SPID = spid;
            else
                SPID = SqlProcConnect.UpdateSpid(this.SqlConnection);
        }

        public Context Open(bool withCommand = false)
            => AssureOpen(withCommand) ? this : null;

        public static T Result<T>(Task task)
        {
            Task<T> taskT = task is Task<T> ? (Task<T>)task : null;
            var obj = taskT == null ? task.AsyncState : taskT.GetAwaiter().GetResult();
            return obj is T ? (T)obj : default(T);
        }

        public Task<Context> OpenAsync(bool withCommand = false)
            => Result<bool>(AssureOpenAsync(withCommand))
             ? Task.FromResult(this) : null; // new Task(() =>{}); // Task.FromCanceled(CancellationToken.None);


        public virtual T AssureOpenDb<T>(bool withCommand = false) where T : Context
        {
            if (!AssureOpen()) {
                return null;
            }

            return this as T;
        }

        public virtual bool AssureOpen(bool withCommand = false)
        {
            var conn = SqlConnection;
            conn = CheckConn(conn);
            Guard.CheckNotNull(conn);
            if (!withCommand && conn.State == ConnectionState.Open) {
                return true;
            }

            if (conn.State != ConnectionState.Open) {
                this.OnBeforeOpen?.Invoke(this, new SqlConnEventArgs(SqlConnection));

                SqlConnection connect = SafeOpen();
                if (connect == null
                    || connect.State == ConnectionState.Closed
                    || connect.State == ConnectionState.Broken)
                    return false;
            }

            if (withCommand) {
                SPID = null;
                try {
                    UpdateSpid(null);
                    LastError = null;
                }
                catch {
                    var connSql = this.SqlConnection;
                    if (connSql != null && !IsDisposed)
                        connSql.Close();
                    SetProxy(ConnectionPool.NewConn(this.ConnectionString()));
                }

                if (SqlConnection != null && SqlConnection.State != ConnectionState.Open) {
                    return false; // Failure
                }

                // Retry
                try {
                    if (SPID == null)
                        UpdateSpid(null);
                }
                catch (Exception ex) { LastError = new Exception("Retry connect error", ex); }
            }

            OnAfterOpen?.Invoke(this, new SqlConnEventArgs(SqlConnection));

            if (_proxy == null && !IsDisposed) {
                SetProxy(base.Connection as SqlConnection);
            }
            if (LastConnString == null || SqlConnection != null) {
                SetLastConnString(SqlConnection.ConnectionString);
            }

            return SqlConnection.State == ConnectionState.Open;
        }

        public SqlConnection SafeOpen()
        {
            LastError = null;
            SqlConnection connect = null;

            if (!IsDisposed && string.IsNullOrWhiteSpace(SqlConnection.DataSource)) {
                ConnectionPool.CloseConn(base.Connection);
                _proxy = ConnectionPool.NewConn(this.ConnectionString());
                if (string.IsNullOrWhiteSpace(_proxy.DataSource)) {
                    return connect;
                }
            }

            try {
                connect = SqlConnection;
                connect.Open();
                if (!string.IsNullOrWhiteSpace(DbName) && connect.Database != DbName)
                    connect.ChangeDatabase(DbName);
                else
                    dbName = connect.Database;
            }
            catch (Exception ex) { LastError = ex; }
            return connect;
        }

        public virtual Task<bool> AssureOpenAsync(bool withCommand = false)
        {
            var conn = SqlConnection;
            
            conn = CheckConn(conn);
            Task<SqlConnection> connTask = SafeOpenAsync();

            if (connTask.Exception == null) {
                connTask.GetAwaiter().GetResult();
            }
            
            if (connTask.Exception != null) {
                // Parse error:
                var aggr = connTask.Exception as AggregateException;
                this.LastError = aggr?.GetBaseException() ?? connTask.Exception;
                if (this.LastError is SqlException) {
                    // var errList = this.LastError as SqlErrorCollection;
                    throw this.LastError;
                }

                conn = CheckConn(SqlConnection);
            } else {
                conn = connTask.Result;
            }

            if (!withCommand) {
                if (conn.State == ConnectionState.Closed) { 
                    // 
                }
                return Task.FromResult(conn.State == ConnectionState.Open);
            } else {
                Task<object> spid = null;
                var cmd = new SqlCommand { CommandText = "SELECT @@SPID", Connection = conn };

                spid = cmd.ExecuteScalarAsync(CancellationToken.None);
                TaskEx.WaitAll(spid);
                UpdateSpid(spid.Result as int?);
            }
            return Task.FromResult(SPID.HasValue && SPID.Value > 0);
        }

        public Task<SqlConnection> SafeOpenAsync()
        {
            LastError = null;
            SqlConnection connect = null;
            Task connectionTask;
            try {
                connect = SqlConnection;
                connectionTask = connect.OpenAsync();
                TaskEx.WaitAll(connectionTask); //make sure the task is completed

                if (connectionTask.IsFaulted) {  // in case of failure
                    LastError = connectionTask.Exception;
                }

                if (!string.IsNullOrWhiteSpace(DbName) && connect.Database != DbName)
                    connect.ChangeDatabase(DbName);
                else
                    dbName = connect.Database;
            }
            catch (Exception ex) { LastError = ex; }

            // #if NETCORE
            if (LastError != null) {
                return TaskEx.FromException<SqlConnection>(LastError);
            }
            return Task.FromResult(connect);
        }

        protected virtual SqlConnection CheckConn(SqlConnection conn)
        {
            if (conn == null || string.IsNullOrWhiteSpace(conn.DataSource)) {
                _proxy = ConnectionPool.NewConn(this.ConnectionString());
                conn = SqlConnection;
                if (conn == null)
                    throw new ArgumentNullException("Connection create error (NewConn failed)");
            }

            if (conn.State == ConnectionState.Open) {
                LastError = null;
                try {
                    if (SqlConnection.Database != DbName && !string.IsNullOrWhiteSpace(DbName)) {
                        SqlConnection.ChangeDatabase(DbName);
                        // There is already an open DataReader associated with this Command which must be closed first.
                    } else {
                        this.dbName = SqlConnection.Database;
                    }
                }
                catch (Exception ex) {
                    LastError = ex;
                    _proxy = ConnectionPool.NewConn(this.ConnectionString());
                    conn = SqlConnection;
                }
            }
            return conn;
        }

        protected SqlConnection _proxy;

        IDbConnection IDbContext.Connection { get { return SqlConnection; } }
        public virtual SqlConnection SqlConnection
        {
            [DebuggerStepThrough]
            get => _proxy ?? (this.IsDisposed ? null : base.Connection as SqlConnection);
        }

        public virtual SqlConnection SqlConnectionOpen {
            get {
                var conn = SqlConnection;
                return conn != null && conn.State == ConnectionState.Open ? conn : null;
            }
        }

        public void SetProxy(SqlConnection conn)
        {
            if (_proxy != null && _proxy != conn)
                _proxy.Dispose();
            _proxy = conn;
        }

        #endregion

        #region Sql Execute methods

        public virtual ICollection<T> Exec<T>(ISqlProc procedure) where T : class
        {
            if (procedure.Connection == null)
            {
                var conn = SqlConnection;
                if (conn == null)
                    throw new ArgumentNullException("procedure.Connection");
                if (conn.State != ConnectionState.Open)
                    AssureOpen(withCommand: true);

                procedure.Connection = SqlConnection;
                if (procedure.Connection == null)
                    throw new ArgumentNullException("procedure.Connection");
            }

            Trace.WriteLine("SqlProc: " + procedure.CmdText);

            // ICollection<T> list = DbGetHelper.SqlGetCollection<T>(procedure);
            ICollection<T> list = null;

            if (procedure.Connection != null && procedure.CloseOnDispose)
                procedure.Connection.Dispose();

            procedure.Dispose();
            procedure.Connection = null;

            return list;
        }

        public XDocument ExecXml(ISqlProc procedure)
        {
            if (procedure.Connection == null)
                procedure.Connection = SqlConnection;

            AssureOpen();
            var cmd = procedure.CreateCommand() as SqlCommand;
            Trace.WriteLine("ExecXml " + cmd.CommandText);

            XDocument doc = null; // SqlProcConnect.ExecuteXmlReaderRetry(cmd, this);

            return doc;
        }

        // already an open DataReader
        public object ExecuteScallarSafe(string cmdText, params object[] parameters)
        {
            var connection = this.SqlConnection;
            if (connection.State != ConnectionState.Open)
            {
                AssureOpen(true);
                connection = this.SqlConnection;
            }

            object result = null;
            LastError = null;
            // first try
            try
            { result = this.ExecuteScallar(cmdText); }
            catch (Exception ex) { this.LastError = ex; }

            // already an open DataReader
            if (LastError != null && LastError.Message.IndexOf("open DataReader", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                connection.Dispose();
                if (!AssureOpen(true))
                    return null;

                // second try
                try
                { result = this.ExecuteScallar(cmdText); }
                catch (Exception ex) { this.LastError = ex; }
            }

            return result;
        }

        // Unsafe, with exception
        public object ExecuteScallar(string cmdText, params object[] parameters)
        {
            using (var cmd = new SqlCommand(cmdText, this.SqlConnection))
            {
                return cmd.ExecuteScalar();
            }
        }

        // Unsafe, with exception
        public Task<object> ExecuteScalarAsync(string cmdText, params object[] parameters)
        {
            using (var cmd = new SqlCommand(cmdText, this.SqlConnection)) {
                return cmd.ExecuteScalarAsync();
            }
        }

        // was: ExecScalar
        public T ExecuteScallar<T>(string cmdText, params object[] parameters)
        {
            // Convert.ChangeType(ExecuteScallar(cmdText, parameters), typeof(T));
            using (var cmd = new SqlCommand(cmdText, this.SqlConnection))
            {
                return (T)cmd.ExecuteScalar();
            }
        }

        //  ExecuteMapper
        public int ExecuteNonQuery(string cmdText, params object[] parameters)
        {
            using (var cmd = new SqlCommand(cmdText, this.SqlConnection))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public Task<int> ExecuteNonQueryAsync(string cmdText, params object[] parameters)
        {
            using (var cmd = new SqlCommand(cmdText, this.SqlConnection)) {
                return cmd.ExecuteNonQueryAsync();
            }
        }

        #endregion

        //public uint PoolCount { get { return SqlPoolInfo.CounterWmi(); } }
    }

    public class SqlConnEventArgs : EventArgs
    {
        public SqlConnEventArgs(SqlConnection conn) { this.SqlConnection = conn; }

        public SqlConnection SqlConnection { get; private set; }
    }

    //public class MappingContext : MappingSource
    //    //     The type of System.Data.Linq.DataContext on which to base the mapping.
    //    protected override MetaModel CreateModel(Type dataContextType) { return null; }
    //    //     Returns the mapping model.
    //    // public MetaModel GetModel(Type dataContextType) { return null; }

    public class TaskEx
    {
        public static void WaitAll(params Task[]  tasks) 
            => Task.WaitAll(tasks);

        internal static Task<TResult> FromCancellation<TResult>(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested) throw new ArgumentOutOfRangeException("cancellationToken");
            return new Task<TResult>(() => default(TResult), cancellationToken, TaskCreationOptions.None);
        }

        public static Task<TResult> FromException<TResult>(Exception exception)
#if NETSTANDARD2_0 || NET46 // || NETCORE
            => Task.FromException<TResult>(exception);
#else
            => new Task<TResult>(() => default(TResult), CancellationToken.None, TaskCreationOptions.None);
            //  task.TrySetException(exception);
#endif

        //internal Task(Func<TResult> valueSelector, Task parent, CancellationToken cancellationToken
    }

}

