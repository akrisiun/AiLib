using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Ai.Entity
{
    public class SqlText : ISqlProc
    {
        public SqlConnection Connection { get; set; }
        public string CmdText { get; set; }

        public virtual SqlConnection OpenConnection()
        {
            return Connection;
        }

        public virtual SqlCommand CreateCommand()
        {
            var cmd = new SqlCommand(cmdText: CmdText, connection: Connection);
            if (cmd.Connection == null)
                throw new System.ArgumentNullException("Connection");

            cmd.CommandType = CommandType.Text;

            return cmd;
        }

        public IList<T> Exec<T>(IList<T> list, Action<double> progress = null) where T : class
        {
            var helper = new DbDataMapHelper<T>();
            DbGetHelper.ExecFill<T>(this, list, helper, progress);

            return list;
        }

        public IEnumerable<object[]> ExecEnum()
        {
            var helper = new DbDataMapHelper<object[]>();
            return DbGetHelper.ExecEnumerable(this, helper, null);
        }

        public object[] ExecFirst()
        {
            var numerator = ExecEnum();
            if (numerator.Any())
                return numerator.First();
            return null;
        }

        public string ConnectionString()
        {
            return Connection.ConnectionString;
        }

        public string DbName
        {
            get { return Connection.Database; }
        }
    }
}
