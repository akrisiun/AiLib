using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    public class SqlObjTableReader : SqlTableMapper, IDisposable
    {
        #region Properties

        private SqlConnection conn;
        private SqlCommand cmd;
        SqlDataReader dataReader;
        private StateExec state;

        public StateExec State { get { return state; } }
        public SqlConnection Connection { get { return conn; } }
        public SqlDataReader DataReader { get { return dataReader; } }

        public SqlObjTableReader(Action<SqlTableMapper, DbDataReader> propertiesParser)
            : base(propertiesParser)
        {
            conn = null;
            cmd = null;
            dataReader = null;
            state = StateExec.Init;
        }

        public SqlConnection OpenConnection(Context db)
        {
            conn = new SqlConnection(db.ConnectionString());
            conn.Open();
            if (conn.State != ConnectionState.Open)
                return null;

            conn.ChangeDatabase(db.DbName);
            return conn;
        }

        #endregion

        #region Prepare Reader

        public bool Prepare(ISqlProc proc, Action<SqlTableMapper, DbDataReader> parser = null, int? commandTimeout = null)
        {
            if (state != StateExec.Init)
                this.Dispose();
            if (parser != null)
                base.propertiesParser = parser;

            LastRow = null;
            var conn = proc.OpenConnection();
            if (conn == null)
                return false;
            cmd = proc.CreateCommand();
            cmd.Connection = conn;
            if (conn.State != ConnectionState.Open)
                conn.Open();

            if (commandTimeout.HasValue)
                cmd.CommandTimeout = commandTimeout.Value;

            cmd.Prepare();
            dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SingleResult);

            state = StateExec.Prepared;

            if (dataReader.Read())
            {
                // First record
                (this as IDataMapHelper<object[]>).GetProperties(dataReader);

                var helper = this;
                object[] objVal = helper.DbRecordArray();
                // first record array
                dataReader.GetValues(objVal);
                LastRow = helper.SetValues(objVal);
                return true;
            }

            return false;
        }

        public object[] LastRow { get; private set; }

        public object[] FirstRecord(SqlProc proc, Action<SqlTableMapper, DbDataReader> parser = null, int? commandTimeout = null)
        {
            if (parser != null)
                base.propertiesParser = parser;

            LastRow = null;
            var conn = OpenConnection(proc.Context);
            if (conn == null)
                return LastRow;

            using (var cmd = proc.CreateCommand())
            {
                cmd.Connection = conn;
                if (commandTimeout.HasValue)
                    cmd.CommandTimeout = commandTimeout.Value;

                cmd.Prepare();
                var dataReader = cmd.ExecuteReader(CommandBehavior.SingleRow); //  CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);

                (this as IDataMapHelper<object[]>).GetProperties(dataReader);

                // CommandBehavior.
                //  SingleResult = 1,   The query returns a single result set.
                //    SchemaOnly = 2,
                //     The query returns column information only. When using System.Data.CommandBehavior.SchemaOnly,
                //     the .NET Framework Data Provider for SQL Server precedes the statement being
                //     executed with SET FMTONLY ON.
                //  KeyInfo = 4,
                //     The query returns column and primary key information.
                //  SingleRow = 8,
                //     The query is expected to return a single row of the first result set. Execution
                //     may, but are not required to, use this information to optimize the performance
                //     of the command. When you specify System.Data.CommandBehavior.SingleRow with
                //     the System.Data.OleDb.OleDbCommand.ExecuteReader() method of the System.Data.OleDb.OleDbCommand
                //     object, the .NET Framework Data Provider for OLE DB performs binding using
                //     the OLE DB IRow interface if it is available. Otherwise, it uses the IRowset
                //     interface. If your SQL statement is expected to return only a single row, also improve performance.

                var helper = this;
                object[] objVal = helper.DbRecordArray();
                dataReader.GetValues(objVal);
                LastRow = helper.SetValues(objVal);

                cmd.Cancel();
                // The name/value pair "Asynchronous Processing=true" was not included within
                dataReader.Dispose();
            }

            return LastRow;
        }

        public void StateExecuting()
        {
            state = StateExec.Executing;
        }

        public int? Records { get { return this.dataReader == null ? null : (int?)dataReader.RecordsAffected; } }

        #endregion

        public IEnumerable<object[]> Query()
        {
            StateExecuting();

            if (dataReader == null)
            {
                Dispose();
                state = StateExec.Init;

                yield break;
            }
            else
            {
                if (LastRow != null)
                    yield return LastRow;

                var helper = this;
                while (dataReader.Read())
                {
                    object[] objVal = helper.DbRecordArray();
                    dataReader.GetValues(objVal);
                    LastRow = helper.SetValues(objVal);
                    yield return LastRow;
                }

                Dispose();
                state = StateExec.Init;
            }
        }

        public enum StateExec
        {
            Init = 0,
            Prepared = 1,
            Executing = 2
            // Finished = 3
        }

        public void Dispose()
        {
            if (dataReader != null)
                dataReader.Dispose();
            dataReader = null;

            if (cmd != null)
                cmd.Dispose();
            cmd = null;

            if (conn != null && conn.State != ConnectionState.Closed)
                conn.Close();
            if (conn != null)
                conn.Dispose();
            conn = null;
            state = StateExec.Init;
        }
    }    
}
