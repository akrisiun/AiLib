using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ai.Entity;

namespace Ai.Context
{
    public static class SqlProcAsync
    {
        public static async Task<IList<T>> Exec<T>(this SqlProc proc,
               IList<T> list, Action<double> progress) // where T : class
        {
            if (proc.Connection == null)
                throw new System.ArgumentNullException("Connection");

            if (proc.Connection.State != ConnectionState.Open)
                await proc.Connection.OpenAsync();

            using (var command = proc.CreateCommand())
            {
                // Summary:
                //     Initiates the asynchronous execution of the Transact-SQL statement or stored
                //     procedure that is described by this and retrieves one or more result sets
                //     from the server, given a callback procedure and state information.
                //     The name/value pair "Asynchronous Processing=true" was not included within
                //     the connection string defining the connection for this .
                // #region Assembly Microsoft.Threading.Tasks.Extensions.Desktop.dll, v1.0.168.0
                // public static Task<SqlDataReader> ExecuteReaderAsync(this SqlCommand source);
                Task<SqlDataReader> reader = command.ExecuteReaderAsync();

                if (progress != null)
                    progress(0.0);

                using (SqlDataReader dataReader = reader.Result)
                // SqlDataReader dataReader = command.ExecuteReader())
                {
                    var results = list as IList<T>;
                    results.Clear();

                    if (dataReader.Read())
                    {
                        var helper = new DbDataMapHelper<T>().GetProperties(dataReader as DbDataReader);
                        do
                        {
                            object[] objVal = helper.DbRecordArray();
                            int ret = dataReader.GetValues(objVal);

                            T val = helper.SetValues(objVal);
                            results.Add(val);
                        }
                        while (dataReader.Read());
                    }

                    if (progress != null)
                        progress(1.0);

                    return results;
                }
            }
        }

    }
}
