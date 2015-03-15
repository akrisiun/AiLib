using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Ai.Entity
{
    public struct SqlProcData
    {
        public SqlConnection Connection { get; set; }
        public Context Context { get; set; }

        public string CmdText { get; set; }
        public IList<SqlParameter> Param { get; set; }
    }

    public interface ISqlProc
    {
        SqlConnection Connection { get; }
        string CmdText { get; set; }
        SqlCommand CreateCommand();
        SqlConnection OpenConnection();
        string ConnectionString();
        string DbName { get; }
    }

    public class SqlProcText : SqlProc, ISqlProc
    {
        public override SqlCommand CreateCommand()
        {
            var res = base.CreateCommand();
            res.CommandType = CommandType.Text;
            return res;
        }
    }

    public class SqlProc : ISqlProc
    {
        public SqlConnection Connection { get; set; }
        public Context Context { get; set; }

        public string CmdText { get; set; }
        public IList<SqlParameter> Param { get; set; }

        public static SqlParameter AddWithValue(string Name, object Value)
        {
            Type type = Value.GetType();
            var prm = new SqlParameter(Name, type);
            prm.Value = Value;
            return prm;
        }

        public virtual SqlConnection OpenConnection()
        {
            if (this.Connection != null)
            {
                if (Connection.State != ConnectionState.Open)
                    this.Connection.Open();
                return this.Connection;
            }
            Context.AssureOpen();
            return Context.SqlConnection; 
        }
        public virtual string ConnectionString() 
        { return Context == null ? null : Context.ConnectionString(); }

        public virtual string DbName { get { return Context == null ? this.Connection.Database : Context.DbName; } }

        public virtual SqlCommand CreateCommand()
        {
            var cmd = new SqlCommand(cmdText: CmdText, connection: Connection ?? Context.SqlConnection);
            if (cmd.Connection == null)
                throw new System.ArgumentNullException("Connection");

            cmd.CommandType = CommandType.StoredProcedure;

            if (Param != null && Param.Count > 0)
                foreach (var parm in Param)
                    cmd.Parameters.Add((parm as ICloneable).Clone() as SqlParameter);

            return cmd;
        }

        public IList<T> Exec<T>(IList<T> list, Action<double> progress = null) where T : class
        {
            var helper = new DbDataMapHelper<T>();
            DbGetHelper.ExecFill<T>(this, list, helper, progress);

            return list;
        }

        public bool ExecMap<T>(IList<T> list, IDataMapHelper<T> mapper,
                Action<double> progress = null) where T : class
        {
            return DbGetHelper.ExecFill<T>(this, list, mapper, progress);
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

    public static class SqlProcExt
    {
        public static SqlProc CmdText(string cmdText, Context context = null, Action<SqlProc> setup = null)
        {
            var proc = new SqlProc()
            {
                CmdText = cmdText,
                Context = context,
                Connection = context == null ? null : context.SqlConnection,
                Param = null
            };
            if (setup != null)
                setup(proc);
            return proc;
        }

        public static SqlProc WithParam(this SqlProc proc, string name, object value)
        {
            if (proc.Param == null)
                proc.Param = new List<SqlParameter>() { SqlProc.AddWithValue(name, value) };
            else
                if (value != null)
                    proc.Param.Add(SqlProc.AddWithValue(name, value));

            return proc;
        }

        public static SqlProc WithParam(this SqlProc proc, object namedParam)
        {
            if (namedParam != null)
                proc.Param = NameProperties.Parse(namedParam);

            return proc;
        }

        public static SqlProc ProcNamed(object namedParam, Context db = null)
        {
            var properties = new NameProperties(namedParam);
            if (properties.List.Count == 0)
                return null;

            string cmdText = properties.GetValue(namedParam, properties.FirstName()) as string;
            Ai.Guard.Check(cmdText.Length > 4);
            var proc = SqlProcExt.CmdText(cmdText);

            var listParam = new List<SqlParameter>();
            foreach (string itemName in properties.Names(1))
            {
                var val = Ai.Reflection.Utils.GetPropertyValue(namedParam, itemName);
                if (val != null)
                    listParam.Add(SqlProc.AddWithValue("@" + itemName, val));
            }

            proc.Param = listParam;
            proc.Context = db;

            return proc;
        }

    }

}
