using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using Ai;
using Ai.Reflection;

namespace Ai.Entity
{
    public static class SqlMultiDyn
    {

        public static DbEnumeratorData<ExpandoObject> ExecDynCmd(this Context db, SqlCommand execCmd
                    , Action<SqlCommand> setupCmd = null
                    , Action<Exception> onError = null)
        {
            var proc = new SqlProcText
            {
                CmdText = execCmd.CommandText,
                Context = db,
                Connection = db.SqlConnection,
                Param = null
            };

            SqlDataReader readerGet = null;
            Func<SqlDataReader> lazyReader = LazyReader(proc, (reader) => readerGet = reader, setupCmd, onError);
            var numeratorObj = new DbEnumeratorData<ExpandoObject>(lazyReader);
            if (numeratorObj == null)
                return DbEnumeratorData<ExpandoObject>.Empty;

            if (!numeratorObj.ReaderAvailable)
            {
                numeratorObj.Reset();
            }
            return numeratorObj;
        }

        // public static IEnumerator<ExpandoObject> 
        public static DbEnumeratorData<ExpandoObject> ResultDyn(this ISqlProc proc
            , Action<SqlDataReader> readerGet, Action<SqlCommand> setupCmd = null
            , Action<Exception> onError = null)
        {
            Func<SqlDataReader> lazyReader = LazyReader(proc, readerGet, setupCmd, onError);
            var dynNumerator = new DbEnumeratorData<ExpandoObject>(lazyReader);
            return dynNumerator; // as IEnumerator<ExpandoObject>;
        }

        public static Func<SqlDataReader> LazyReader(this ISqlProc proc
            , Action<SqlDataReader> readerGet
            , Action<SqlCommand> setupCmd = null
            , Action<Exception> onError = null)
        {
            return new Func<SqlDataReader>(() =>
                {
                    SqlDataReader reader = null;
                    string errorStr = null;
                    try
                    {
                        reader = ExecMultiReader(proc, setupCmd, onError: null, progress: null);
                    }
                    catch (Exception ex)
                    {
                        errorStr = String.Format("error in sql {0}: {1}", proc.CmdText ?? "???", ex.Message);
                        System.Diagnostics.Trace.Write(errorStr);
                        if (onError != null)
                            onError(ex);
                    }

                    // System.AppDomain.CurrentDomain.UnhandledException 
                    if (reader == null || reader.IsClosed || reader.Depth != 0)
                        return null;

                    readerGet(reader);
                    return reader;
                });
        }

        // Unsafe
        public static SqlDataReader ExecMultiReader(this ISqlProc proc,
                          Action<SqlCommand> setup = null,
                          Action<Exception> onError = null,
                          Action<double> progress = null)
        {
            var command = proc.CreateCommand();
            //if (command.Connection == null || command.Connection.State != ConnectionState.Open)

            var connStr = proc.ConnectionString();
            Guard.Check(!string.IsNullOrWhiteSpace(connStr), "Exec MultiReader connection error");

            SqlConnection connection = new SqlConnection(connStr);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                if (connection.State != ConnectionState.Open)
                    return null;
            }
            if (connection.Database != proc.DbName)
                connection.ChangeDatabase(proc.DbName);
            command.Connection = connection;

            if (setup != null)
                setup(command);

            if (progress != null)
                progress(0.0);

            SqlDataReader dataReader = null;
            if (onError != null)
                try
                {
                    dataReader = command.ExecuteReader(behavior: CommandBehavior.CloseConnection);
                }
                catch (Exception ex) { onError(ex); }
            else
                dataReader = command.ExecuteReader(behavior: CommandBehavior.CloseConnection);

            if (dataReader != null && dataReader.IsClosed) // !dataReader.Read())
            {
                if (progress != null)
                    progress(1.0);

                return null;
            }

            return dataReader;
        }

    }

}