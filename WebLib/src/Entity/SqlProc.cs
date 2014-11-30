using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ai.Entity
{
    public struct SqlProc
    {
        public SqlConnection Connection { get; set; }
        public DbContext Context { get; set; }

        public string CmdText { get; set; }
        public IList<SqlParameter> Param { get; set; }

        public static SqlParameter AddWithValue(string Name, object Value)
        {
            Type type = Value.GetType();
            var prm = new SqlParameter(Name, type);
            prm.Value = Value;
            return prm;
        }

        public SqlCommand CreateCommand()
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
            Ai.Assert.IsTrue(list != null, "SqlProc.Exec list null error");

            var helper = new DbDataMapHelper<T>();
            DbGetHelper.ExecFill<T>(this, list, helper, progress);

            return list;
        }

        public bool ExecMap<T>(IList<T> list, IDataMapHelper<T> mapper,
                Action<double> progress = null) where T : class
        {
            return DbGetHelper.ExecFill<T>(this, list, mapper, progress);
        }

    }

    public static class SqlProcExt
    {
        public static SqlProc CmdText(string cmdText, DbContext context = null)
        {
            return new SqlProc()
            {
                CmdText = cmdText,
                Context = context,
                Connection = context == null ? null : context.SqlConnection,
                Param = null
            };
        }

        public static SqlProc WithParam(this SqlProc proc, string name, object value)
        {
            if (proc.Param == null)
                proc.Param = new List<SqlParameter>() { SqlProc.AddWithValue(name, value) };
            else
                proc.Param.Add(SqlProc.AddWithValue(name, value));

            return proc;
        }


    }

}
