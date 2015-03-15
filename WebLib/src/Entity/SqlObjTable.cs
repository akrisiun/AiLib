using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Ai.Entity
{
    public class SqlObjTable : IEnumerable<object[]>
    {
        #region ctor
        public SqlField[] Fields { get; set; }

        private IQueryable<object[]> querySource;
        private SqlObjQueryParameters queryParam;
        // public abstract DbContext Reconnect { get; }

        public bool RowsAny
        {
            get
            {
                if (queryParam.IsEmpty()
                    || queryParam.DataReader == null)
                    return false;
                if (!queryParam.DataReader.HasRows)
                    return false;

                return true;
            }
        }

        public IEnumerable<object[]> Rows
        {
            get
            {
                if (!Reset())
                    return Enumerable.Empty<object[]>();        // error
                return this as IEnumerable<object[]>;
            }
        }

        // todo: IList<object[]> Rows

        public SqlObjTable()
        {
            Fields = new SqlField[] { };

            queryParam = SqlObjQueryParameters.WithParser(
                new SqlProc() { CmdText = String.Empty, Context = null }, null);

            querySource = Enumerable.Empty<object[]>().AsQueryable();

            // Queryable.
            // Rows = new List<object[]>();
        }

        public virtual IEnumerable<SqlField> FieldsVisible()
        {
            foreach (var field in Fields)
                if (!field.Hide)
                    yield return field;
        }

        public virtual IEnumerable<object[]> RowsFromTake(int indexFrom, int rowsTake)
        {
            // var query = Rows.AsQueryable<object[]>();
            var query = this.Query;
            if (indexFrom > 0)
                query = query.Skip(indexFrom);
            if (rowsTake > 0)
                query = query.Take(rowsTake);

            foreach (var rowItem in query)
                yield return rowItem;
        }

        #endregion

        #region IEnumerator

        public virtual IEnumerator<object[]> GetEnumerator()
        {
            if (!queryParam.IsPrepared())
                yield break;

            var query = this.queryParam.Query();
            foreach (object[] rowItem in query)
                yield return rowItem;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        public IQueryable<object[]> Query
        {
            get
            {
                if (!queryParam.IsPrepared())
                    queryParam.Prepare(queryParam.proc);

                var enumerable = queryParam.Query();
                this.querySource = enumerable.AsQueryable<object[]>();
                return this.querySource;
            }
        }

        #region Sql

        public bool IsClosed()
        {
            return Fields.Length == 0 || queryParam.proc.Connection == null;
        }

        public bool Reset()
        {
            if (this.Fields.Length == 0)
                return false;

            this.querySource = this.Query;
            return this.querySource != null;
        }

        public bool ExecNamed(DbContext context, object paramNamed, int iFrom = 0, int iTake = 0)
        {
            var prop = new NameProperties(paramNamed);
            string execName = prop.GetValue(paramNamed, prop.FirstName()) as string;
            var proc = SqlProcExt.CmdText(execName, context);

            var paramList = new List<SqlParameter>();
            foreach (string item in prop.Names(1))
                paramList.Add(SqlProc.AddWithValue("@" + item, prop.GetValue(paramNamed, item)));
            if (paramList.Count > 0)
                proc.Param = paramList;

            return ExecProc(proc, iFrom, iTake);
        }

        public bool Exec(DbContext context, string[] param, int iFrom = 0, int iTake = 0)
        {
            string execName = param[0];

            var proc = SqlProcExt.CmdText(execName, context);

            return ExecProc(proc, iFrom, iTake);
        }

        private bool ExecProc(SqlProc proc, int iFrom = 0, int iTake = 0)
        {
            var fld = this.Fields;

            // && Rows.Count == 0
            Ai.Assert.IsTrue(Rows != null, "SqlTable rows error");

            Action<SqlTableMapper, DbDataReader> parser = (map, dbReader) =>
            {
                int len = dbReader.FieldCount;
                Array.Resize<SqlField>(ref fld, len);
                for (int i = 0; i < len; i++)
                {
                    fld[i] = new SqlField()
                    {
                        Ordinal = i,
                        Name = dbReader.GetName(i),
                        Type = dbReader.GetFieldType(i),
                        Caption = dbReader.GetName(i),
                        Hide = false
                    };
                }
                this.Fields = fld;
            };

            bool init = this.queryParam.Prepare(proc, parser);
            Ai.Assert.IsTrue(fld.Length > 0);

            return init;
            // return enumerable.Any<object[]>();
        }

        #endregion
    }

    public class NameProperties
    {
        public NameProperties(object paramNamed)
        {
            list =  Reflection.Utils.GetProperties(paramNamed); // .GetPropertyValue  ReflectionUtils.GetProperties(paramNamed);
        }

        PropertyDescriptorCollection list;
        public PropertyDescriptorCollection List { get { return list; } }

        public IEnumerable<string> Names(int iFrom = 1)
        {
            foreach (PropertyDescriptor item in list)
            {
                if (list.IndexOf(item) >= iFrom)
                    yield return item.Name;
            }
        }

        public string FirstName()
        {
            if (list == null || list.Count == 0) return null;

            return list[0].Name;
        }

        public object GetValue(object paramObj, string propertyName)
        {
            return Reflection.Utils.GetPropertyValue(paramObj, propertyName);
        }

    }

    public class SqlTableMapper : IDataMapHelper<object[]>
    {
        public SqlTableMapper(Action<SqlTableMapper, DbDataReader> propertiesParser)
        {
            this.propertiesParser = propertiesParser;
        }
        protected Action<SqlTableMapper, DbDataReader> propertiesParser;

        public Type Type { get { return typeof(object[]); } }
        public int FieldCount { get { return iLen; } }

        int iLen;
        string[] FieldNames;

        IDataMapHelper<object[]> IDataMapHelper<object[]>.GetProperties(DbDataReader dataReader)
        {
            iLen = dataReader.FieldCount;
            FieldNames = new string[iLen];
            for (int i = 0; i < iLen; i++)
                FieldNames[i] = dataReader.GetName(i);

            if (propertiesParser != null)
                propertiesParser(this, dataReader);
            return this;
        }

        public object[] SetValues(object[] objVal)
        {
            return objVal;
        }

        public object[] DbRecordArray()
        {
            return (object[])Array.CreateInstance(typeof(object), iLen);
        }

        public virtual int? GetOrdinal(string columnName)
        {
            for (int i = 0; i < FieldNames.Length; i++)
                if (FieldNames[i] == columnName)
                    return i;
            return null;
        }

        public virtual object GetField(string columnName, object[] arrayItem)
        {
            int? find = GetOrdinal(columnName);
            return find.HasValue ? arrayItem[find.Value] != DBNull.Value ? arrayItem[find.Value] : null
                                 : null;
        }
    }

    public struct SqlObjQueryParameters
    {
        public SqlProc proc { get; private set; }
        private SqlObjectReader mapper;

        public static SqlObjQueryParameters WithParser(SqlProc proc, Action<SqlTableMapper, DbDataReader> propertiesParser = null)
        {
            return new SqlObjQueryParameters() { proc = proc, mapper = new SqlObjectReader(propertiesParser) };
        }

        public bool IsEmpty() { return string.IsNullOrWhiteSpace(proc.CmdText) || mapper == null; }
        public bool IsPrepared() { return !IsEmpty() && mapper.DataReader != null; }
        public SqlDataReader DataReader { get { return IsPrepared() ? mapper.DataReader : null; } }

        public bool Prepare(SqlProc proc, Action<SqlTableMapper, DbDataReader> parser = null)
        {
            this.proc = proc;
            return IsEmpty() || mapper.Prepare(proc, parser);
        }

        public IEnumerable<object[]> Query()
        {
            if (!IsPrepared())
                yield break;
            else
            {
                foreach (var item in mapper.Query())
                    yield return item;

                mapper.Dispose();
            }
        }

    }

    public class SqlObjectReader : SqlTableMapper, IDisposable
    {
        private SqlConnection conn;
        private SqlCommand cmd;
        SqlDataReader dataReader;
        private StateExec state;

        public StateExec State { get { return state; } }
        public SqlConnection Connection { get { return conn; } }
        public SqlDataReader DataReader { get { return dataReader; } }

        public SqlObjectReader(Action<SqlTableMapper, DbDataReader> propertiesParser)
            : base(propertiesParser)
        {
            conn = null;
            cmd = null;
            dataReader = null;
            state = StateExec.Init;
        }

        public bool Prepare(SqlProc proc, Action<SqlTableMapper, DbDataReader> parser = null)
        {
            if (state != StateExec.Init)
                this.Dispose();

            Context db = proc.Context;
            conn = new SqlConnection(db.ConnectionString());
            conn.Open();
            if (conn.State != ConnectionState.Open)
                return false;

            conn.ChangeDatabase(db.DbName);

            cmd = proc.CreateCommand();
            cmd.Connection = conn;

            dataReader = cmd.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);

            if (parser != null)
                base.propertiesParser = parser;
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
            //     of the query may affect the database state. Some .NET Framework data providers
            //     may, but are not required to, use this information to optimize the performance
            //     of the command. When you specify System.Data.CommandBehavior.SingleRow with
            //     the System.Data.OleDb.OleDbCommand.ExecuteReader() method of the System.Data.OleDb.OleDbCommand
            //     object, the .NET Framework Data Provider for OLE DB performs binding using
            //     the OLE DB IRow interface if it is available. Otherwise, it uses the IRowset
            //     interface. If your SQL statement is expected to return only a single row,
            //     specifying System.Data.CommandBehavior.SingleRow can also improve application
            //     performance. It is possible to specify SingleRow when executing queries that
            //     are expected to return multiple result sets. In that case, where both a multi-result
            //     set SQL query and single row are specified, the result returned will contain
            //     only the first row of the first result set. The other result sets of the
            //     query will not be returned.

            dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SingleResult);

            state = StateExec.Prepared;
            return true;
        }

        public void StateExecuting()
        {
            state = StateExec.Executing;
        }

        public int? Records { get { return this.dataReader == null ? null : (int?)dataReader.RecordsAffected; } }

        public IEnumerable<object[]> Query()
        {
            StateExecuting();
            // using (SqlDataReader dataReader = cmd.ExecuteReader())
            if (dataReader == null || !dataReader.Read())
            {
                Dispose();
                state = StateExec.Init;

                yield break;
            }
            else
            {
                var helper = this;
                do
                {
                    object[] objVal = helper.DbRecordArray();
                    dataReader.GetValues(objVal);

                    yield return helper.SetValues(objVal);
                }
                while (dataReader.Read());

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
