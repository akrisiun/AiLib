using Ai.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Ai.Entity
{
    public class SqlObjTable : IEnumerable<object[]>
    {
        public bool ExecNamed(Context context, object paramNamed, int iFrom = 0, int iTake = 0)
        {
            NameProperties prop = new NameProperties(paramNamed);
            string execName = prop.GetValue(paramNamed, prop.FirstName()) as string;
            var proc = SqlProcExt.CmdText(execName, context);
            if (prop.List.Count > 1)
                proc.Param = NameProperties.Parse(paramNamed, 1);

            return ExecProc(proc, iFrom, iTake);
        }

        #region ctor
        public SqlField[] Fields { get; set; }

        private IQueryable<object[]> querySource;
        private SqlQueryParameters queryParam;
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

        public virtual IEnumerable<object[]> Rows
        {
            get
            {
                if (queryParam.IsEmpty())
                    return Enumerable.Empty<object[]>();        // error
                return this as IEnumerable<object[]>;
            }
        }

        // IList<object[]> Rows

        public SqlObjTable()
        {
            Fields = new SqlField[] { };

            queryParam = SqlQueryParameters.WithParser(
                new SqlProc() { CmdText = String.Empty, Context = null }, null);

            querySource = Enumerable.Empty<object[]>().AsQueryable();
        }

        public virtual IEnumerable<SqlField> FieldsVisible
        {
            get
            {
                int index = 0;
                foreach (var field in Fields)
                    if (!field.Hide)
                    {
                        if (field.OrdinalVisible != index)
                            field.OrdinalVisible = index;
                        index++;
                        yield return field;
                    }
            }
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

        #region IEnumerable

        public virtual IEnumerator<object[]> GetEnumerator()
        {
            // foreach (object[] rowItem in Rows)
            //     yield return rowItem;
            if (queryParam.DataReader == null)
                Reset();

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

        public int Timeout { get; set; }

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

        public bool Exec(Context context, string[] param, int iFrom = 0, int iTake = 0)
        {
            string execName = param[0];

            var proc = SqlProcExt.CmdText(execName, context);

            return ExecProc(proc, iFrom, iTake);
        }

        public bool ExecNamed(Context context, object namedParam)
        {
            // NameProperties prop = new NameProperties(namedParam);
            var proc = SqlProcExt.ProcNamed(namedParam);
            proc.Context = context;

            return ExecProc(proc);
        }


        private bool ExecProc(ISqlProc proc, int iFrom = 0, int iTake = 0)
        {
            Ai.Guard.Check(proc.CmdText.Length > 0, "SqlTable proc error");
            Ai.Guard.Check(Fields != null, "SqlTable Fields error");

            var fld = this.Fields;

            Action<SqlTableMapper, DbDataReader> parser = (map, dbReader) =>
            {
                fld = SqlFieldArray.GetArray(dbReader);
                this.Fields = fld;
            };

            bool init = this.queryParam.Prepare(proc, parser, this.Timeout);
            Guard.Check(fld.Length > 0, "fld.Length error");

            return init;
        }

        #endregion
    }

    public class NameProperties
    {
        public static List<SqlParameter> Parse(object namedParam, int skipFrom = 0)
        {
            var properties = new NameProperties(namedParam);
            if (properties.List.Count == 0)
                return null;

            var listParam = new List<SqlParameter>();

            foreach (string itemName in properties.Names(skipFrom))
            {
                var val = Utils.GetPropertyValue(namedParam, itemName);
                if (val != null)
                    listParam.Add(SqlProc.AddWithValue("@" + itemName, val));
            }

            return listParam;
        }


        public NameProperties(object paramNamed)
        {
            // list = Reflection.ReflectionUtils.GetProperties(paramNamed);
            list = TypeDescriptor.GetProperties(paramNamed);
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
            return Ai.Reflection.Utils.GetPropertyValue(paramObj, propertyName);
        }

    }

    public struct SqlQueryParameters
    {
        public ISqlProc proc { get; private set; }
        private SqlObjTableReader mapper;

        public static SqlQueryParameters WithParser(ISqlProc proc, Action<SqlTableMapper, DbDataReader> propertiesParser = null)
        {
            return new SqlQueryParameters() { proc = proc, mapper = new SqlObjTableReader(propertiesParser) };
        }

        public bool IsEmpty() { return string.IsNullOrWhiteSpace(proc.CmdText) || mapper == null; }
        public bool IsPrepared() { return !IsEmpty() && mapper.DataReader != null; }
        public SqlDataReader DataReader { get { return IsPrepared() || Prepare() ? mapper.DataReader : null; } }

        public bool Prepare(ISqlProc proc, Action<SqlTableMapper, DbDataReader> parser = null, int? commandTimeout = null)
        {
            this.proc = proc;
            return IsEmpty() || mapper.Prepare(proc, parser, commandTimeout);
        }

        public bool Prepare()
        {
            return IsEmpty() || mapper.Prepare(this.proc);
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
}
