using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    public static class SqlProcResult
    {
        public static DataTable Result(this SqlProc proc)
        {
            var cmd = proc.CreateCommand();

            var table = new DataTable();

            if (cmd.Connection.State != ConnectionState.Open)
            {
                Assert.IsTrue(cmd.Connection.ConnectionString.Length > 0);
                cmd.Connection.Open();
                if (cmd.Connection.State != ConnectionState.Open)
                    return null;
            }

            var reader = cmd.ExecuteReader();

            // Load(IDataReader reader, LoadOption loadOption);
            table.Load(reader, LoadOption.OverwriteChanges);

            return table;
        }

    }
}
