using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ai.Entity
{
    // DbDataMapHelper
    // IDataMapHelper

    public static class DbGetHelper
    {
        // in T
        public static bool ExecFill<T>(this SqlProc proc, IList<T> list,
                    IDataMapHelper<T> mapper,
                    Action<double> progress = null) where T : class // , TMap : IDataMapHelper
        {
            Ai.Assert.IsTrue(proc.Connection != null, "proc.Connection null error in ExecFill");
            Ai.Assert.IsTrue(list != null, "list null error in ExecFill");
            if (proc.Connection.State != ConnectionState.Open)
                proc.Connection.Open();

            using (var command = proc.CreateCommand())
            {
                if (progress != null)
                    progress(0.0);

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    var results = list as IList<T>;
                    if (!dataReader.Read())
                    {
                        if (progress != null)
                            progress(1.0);
                        return false;
                    }

                    // Task<T> GetFieldValueAsync<T>(int i, CancellationToken cancellationToken);
                    var helper = mapper; //  new DbDataMapHelper<T>();
                    helper.GetProperties(dataReader);

                    do
                    {
                        object[] objVal = helper.DbRecordArray();
                        // int ret = 
                        dataReader.GetValues(objVal);
                        T val = helper.SetValues(objVal);
                        results.Add(val);
                    }
                    while (dataReader.Read());

                    if (progress != null)
                        progress(1.0);

                    return true;
                }
            }
        }


        public static IEnumerable<object[]> ExecEnumerable(this SqlProc proc, 
                    IDataMapHelper<object[]> mapper,
                    Action<double> progress = null)
        {
            Ai.Assert.IsTrue(proc.Connection != null, "proc.Connection null error in ExecFill");
            // Ai.Assert.IsTrue(list != null, "list null error in ExecFill");

            using (SqlConnection connection = new SqlConnection(proc.Context.ConnectionString()))
            {
                proc.Connection = connection;
                if (proc.Connection.State != ConnectionState.Open)
                {
                    proc.Connection.Open();
                    if (proc.Connection.State != ConnectionState.Open)
                        yield break;
                }
                if (proc.Connection.Database != proc.Context.DbName)
                    proc.Connection.ChangeDatabase(proc.Context.DbName);

                using (var command = proc.CreateCommand())
                {
                    if (progress != null)
                        progress(0.0);

                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        if (!dataReader.Read())
                        {
                            if (progress != null)
                                progress(1.0);

                            yield break;
                        }

                        // Task<T> GetFieldValueAsync<T>(int i, CancellationToken cancellationToken);
                        var helper = mapper; //  new DbDataMapHelper<T>();
                        helper.GetProperties(dataReader);
                        //  int records = dataReader.RecordsAffected;

                        do
                        {
                            object[] objVal = helper.DbRecordArray();
                            dataReader.GetValues(objVal);

                            object[] val = helper.SetValues(objVal);
                            yield return val;
                        }
                        while (dataReader.Read());

                        if (progress != null)
                            progress(1.0);
                    }
                }
            }
        }

    }

    public class DbObject : DbDataMapHelper<object[]>
    {
        public static DbObject Exec(SqlProc proc, Action<double> progress = null)
        {
            var mapper = new DbObject();
            mapper.Worker = DbGetHelper.ExecEnumerable(proc, mapper, progress).GetEnumerator();
            if (mapper.Worker == null)
                return null;
            return mapper;
        }

        public object MoveNextField(string columnName)
        {
            if (!Worker.MoveNext())
                return null;
            return GetField(columnName, Worker.Current);
        }

        public IEnumerator<object[]> Worker;

        public object[] FirstRecord {
            get
            {
                if (Worker.MoveNext())
                    return Worker.Current;
                return null;
            }
        }
    }

}
