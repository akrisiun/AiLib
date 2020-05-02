using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Dotnet.Entity
{
    public class SqlCmdProc : ISqlProc, ILastError
    {
        public SqlCommand Cmd { get; set; }
        IDbCommand ISqlProc.LastCommand { get { return Cmd; } }
        public Context Context { get; set; }

        public bool CloseOnDispose { get; set; }
        public string CmdText { get; set; }
        public IDbConnection Connection { get { return Cmd.Connection; } set { Cmd.Connection = value as SqlConnection; } }
        public string DbName { get { return Connection.Database; } }
        public Exception LastError { get; set; }

        public string ConnectionString() => Connection.ConnectionString;
        public IDbCommand CreateCommand() => Cmd;
        public void Dispose() { if (CloseOnDispose) Cmd.Dispose(); }
        public IDataReader ExecuteReader(IDbCommand cmd) => Cmd.ExecuteReader();
        public Task<SqlDataReader> ExecuteReaderAsync(IDbCommand cmd) => Cmd.ExecuteReaderAsync();
        public IDbConnection OpenConnection() { return Cmd.Connection; }
    }

    public class SqlProcText : SqlProc, ISqlProcContext, ILastError
    {
        public string Text
        {
            get { return base.CmdText; }
            set { base.CmdText = value; }
        }

        public override IDbCommand CreateCommand()
        {
            var res = base.CreateCommand();
            res.CommandType = CommandType.Text;
            if (!String.IsNullOrWhiteSpace(Text))
                res.CommandText = Text;
            return res;
        }

        public override string ConnectionString()
        { return Connection == null ? null : this.Connection.ConnectionString; }

        public new void Dispose() { base.Dispose(); }

        public override string ToString()
        {
            return Text;
        }
    }

    public class SqlParameterClone
    {
        public static SqlParameter Clone(SqlParameter source)
        {
            // var clone = new SqlParameterClone().MemberwiseClone();
            var clone = new SqlParameter
            {
                DbType = source.DbType,
                Direction = source.Direction,
                SqlValue = source.SqlValue,
                ParameterName = source.ParameterName,
                SourceColumn = source.SourceColumn,
                SourceColumnNullMapping =  source.SourceColumnNullMapping,
                IsNullable = source.IsNullable,
                LocaleId = source.LocaleId
            };
            return clone;
        }
    }

    public class SqlProc : ISqlProcContext
    {
        public IDbConnection Connection { get; set; }
        public bool CloseOnDispose { get; set; }

        public Context Context { get; set; }
        ISqlContext ISqlProcContext.Context { get { return this.Context; } set { this.Context = (Context)value; } }

        Exception _lastError;
        public Exception LastError
        {
            get { return _lastError ?? (Context == null ? null : Context.LastError); }
            set
            {
                _lastError = value;
                if (Context != null)
                    Context.LastError = _lastError;
            }
        }

        public string CmdText { get; set; }
        public IList<SqlParameter> Param { get; set; }

        public static SqlParameter AddWithValue(string Name, object Value)
        {
            Type type = Value.GetType();
            var prm = new SqlParameter(Name, type);
            prm.Value = Value;
            return prm;
        }

        public static SqlParameter AddWithOutput(string Name, DbType dbtype, int size = 0
            , ParameterDirection direction = ParameterDirection.Output)
        {
            var prm = new SqlParameter
            {
                ParameterName = Name,
                DbType = dbtype,
                Size = size,
                Direction = direction
            };
            return prm;
        }

        public virtual IDbConnection OpenConnection()
        {
            if (this.Connection != null)
            {
                if (Context != null && Connection.State == ConnectionState.Closed)
                {
                    Context.AssureOpen(true);
                    this.CloseOnDispose = true;
                    this.Connection = Context.SqlConnection;
                }

                if (Connection.State != ConnectionState.Open)
                {
                    try
                    {
                        this.Connection.Open();
                        this.CloseOnDispose = true;
                    }
                    catch {
                        if (this.Connection != null && Connection.State != ConnectionState.Open)
                            this.Connection = null;
                    }
                }
                if (this.Connection != null)
                {
                    return this.Connection;
                }
            }

            try
            {
                if (Context.SqlConnection != null && Context.SqlConnection.State != ConnectionState.Open)
                    Context.SqlConnection.Dispose();

                Context.AssureOpen(withCommand: this.CloseOnDispose);

                this.Connection = Context.SqlConnection;
            }
            catch
            {
                if (Context != null) {
                    var db = Context.DbName;
                    Context.SetProxy(ConnectionPool.NewConn(Context.LastConnString));
                    Context.AssureOpen(true);

                    Context.SqlConnection.ChangeDatabase(db);
                    this.Connection = Context.SqlConnection;
                }
            }

            return this.Connection;
        }

        public virtual void Dispose()
        {
            if (Connection != null && CloseOnDispose
                && (Context == null || Context.SqlConnection != Connection))
                Connection.Dispose();
        }

        public virtual string ConnectionString()
            => Context == null ? null : Context.ConnectionString();

        public virtual string DbName { get => Context == null ? this.Connection.Database : Context?.DbName; } 

        // ISqlProc.
        public IDbCommand LastCommand { get; protected set; }

        public virtual IDbCommand CreateCommand()
        {
            var cmd = new SqlCommand(cmdText: CmdText,
                connection: (Connection ?? Context.SqlConnection) as SqlConnection);
            if (cmd.Connection == null)
                throw new System.ArgumentNullException("Connection");

            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection = this.PreparedSqlConnection();

            if (cmd.CommandText != null && cmd.CommandText.TrimStart().StartsWith("select ", StringComparison.OrdinalIgnoreCase))
                cmd.CommandType = CommandType.Text;
            else
                cmd.CommandType = CommandType.StoredProcedure;

            if (Param != null && Param.Count > 0)
            {
                foreach (var parm in Param)
                    cmd.Parameters.Add(SqlParameterClone.Clone(parm));
                // cmd.Parameters.Add((parm as object).MemberwiseClone() 
                // (parm as ICloneable).Clone() as SqlParameter);
            }

            // IDbCommand LastCommand { get; }
            this.LastCommand = cmd;
            return cmd;
        }

        public virtual SqlDataReader ExecuteReader(SqlCommand cmd)
            => cmd.ExecuteReader(this.CloseOnDispose ? CommandBehavior.CloseConnection : CommandBehavior.Default);

        public Task<SqlDataReader> ExecuteReaderAsync(IDbCommand cmd) 
            => (cmd as SqlCommand).ExecuteReaderAsync(this.CloseOnDispose ? CommandBehavior.CloseConnection : CommandBehavior.Default);

        IDataReader ISqlProc.ExecuteReader(IDbCommand cmd) { return this.ExecuteReader(cmd as SqlCommand) as IDataReader; }

        public override string ToString()
        {
            return "EXEC " + (CmdText ?? "?")
                 + (Param != null ? " " + String.Concat(Extract(Param)) : string.Empty);
        }

        public static IEnumerable<string> Extract(IEnumerable<SqlParameter> paramList)
        {
            if (paramList == null)
                yield break;

            var numer = paramList.GetEnumerator();
            int iNum = 0;
            while (numer.MoveNext())
                yield return
                    ((++iNum) > 1 ? ", " : string.Empty)
                    + numer.Current.ParameterName + " = \'" + (numer.Current.Value ?? "") + "\'";
        }

        public static IList<SqlParameter> CloneParam(SqlParameterCollection parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return null;

            var res = new List<SqlParameter>();
            var numerator = parameters.GetEnumerator();
            while (numerator.MoveNext())
                res.Add(numerator.Current as SqlParameter);
            return res;
        }
    }


    public static class SqlProcConnect
    {
        public static bool IsReady(this IDbConnection conn)
        {
            return conn != null && conn.State == ConnectionState.Open;
        }

        public static SqlConnection PreparedSqlConnection(this ISqlProc proc)
        {
            if (proc == null)
                return null;
            if (proc.Connection.IsReady())
                return proc.Connection as SqlConnection;

            proc.Connection = proc.OpenConnection();
            proc.CloseOnDispose = true;

            if (proc is ISqlProcContext && string.IsNullOrWhiteSpace(proc.Connection.ConnectionString))
            {
                var procDb = proc as ISqlProcContext;
                proc.Connection = procDb.Context.SqlConnection;
            }

            return proc.Connection as SqlConnection;
        }

        public static Int32? UpdateSpid(this SqlConnection SqlConnection)
        {
            var cmd = new SqlCommand { CommandText = "SELECT @@SPID", Connection = SqlConnection };
            var res = cmd.ExecuteScalar();
            if (res != null && (res is int || res is Int16))
                return Convert.ToInt32(res);

            return null;
        }

        public static Int32? UpdateSpidIfError(this SqlConnection SqlConnection, ILastError onError)
        {
            try
            {
                var cmd = new SqlCommand { CommandText = "SELECT @@SPID", Connection = SqlConnection };
                var res = cmd.ExecuteScalar();
                if (res != null && (res is int || res is Int16))
                    return Convert.ToInt32(res);
            }
            catch (Exception ex) { if (onError != null) onError.LastError = ex; }

            return null;
        }


        public static SqlConnection PreparedSqlConnection(this ISqlContext ctx) { ctx.AssureOpen(true); return ctx.SqlConnection; }

        public static SqlDataReader ExecuteReaderRetry(this SqlCommand command, ILastError errObj, Func<SqlCommand> retry)
        {
            if (errObj != null && errObj.LastError != null)
                errObj.LastError = null;

            SqlDataReader dataReader = null;
            SqlConnection conn = command.Connection;
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                dataReader = command.ExecuteReader() as SqlDataReader;
            }
            catch (Exception ex) { if (errObj != null) errObj.LastError = ex; }

            if (dataReader != null)
                return dataReader;

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
                if (retry != null)
                    command = retry();
                try
                {
                    dataReader = command.ExecuteReader() as SqlDataReader;
                    if (errObj != null) errObj.LastError = null;
                }
                catch (Exception ex) { if (errObj != null) errObj.LastError = ex; }
            }
            return dataReader;
        }

        public static XDocument ExecuteXmlReaderRetry(this SqlCommand cmd, ILastError errObj)
        {
            XDocument doc = null;
            XmlReader reader = null;
            try
            {
                reader = cmd.ExecuteXmlReader();
            }
            catch (Exception ex) { errObj.LastError = ex; }
            if (reader == null)
            {
                if (errObj is ISqlContext)
                    cmd.Connection = PreparedSqlConnection(errObj as ISqlContext);
                else
                    if (errObj is ISqlProc)
                        cmd.Connection = PreparedSqlConnection(errObj as ISqlProc);

                reader = cmd.ExecuteXmlReader();
            }

            if (reader != null)
            {
                errObj.LastError = null;
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
                if (reader is IDisposable)
                    (reader as IDisposable).Dispose();

                cmd.Dispose();
            }
            return doc;
        }

        public static SqlCommand GetSqlCommandReconnect(this ISqlProc proc)
        {
            proc.Connection = null;
            proc.CloseOnDispose = true;
            if (proc is SqlProc)
            {
                var sqlProc = proc as SqlProc;
                if (sqlProc.Context != null)
                {
                    sqlProc.Context.AssureOpen(withCommand: true);
                    sqlProc.Connection = sqlProc.Context.SqlConnection;
                }
                else
                    sqlProc.OpenConnection();
            }
            else
                proc.Connection = proc.OpenConnection();
            return proc.CreateCommand() as SqlCommand;
        }

        public static ISqlProc PrepareWithReconnect(this ISqlProc proc, Context db)
        {
            var conn = proc.Connection;
            if (conn == null)
                proc.Connection = proc.OpenConnection();
            conn = proc.Connection;

            try
            {
                var dbName = conn.Database;
                var cmd = new SqlCommand { CommandText = "SELECT DB_NAME()" };  // neutral command
                var result = cmd.ExecuteScalar() as string;

            }
            catch (Exception ex) { proc.LastError = ex; }
            return proc;
        }

        public static SqlDataReader ExecuteWithReconnect(this ISqlProc proc)
        {
            var lastErr = proc.LastError;
            var cmd = proc.CreateCommand() as SqlCommand;

            var reader = ExecuteReaderRetry(cmd, proc,
                retry: () => GetSqlCommandReconnect(proc));

            lastErr = proc.LastError;
            if (lastErr is SqlException)
                throw lastErr;
            else if (reader == null)
            {
                proc.LastError = new Exception(String.Format("procedure error {0}", proc));
                throw proc.LastError;
            }

            return reader;
        }

    }

}