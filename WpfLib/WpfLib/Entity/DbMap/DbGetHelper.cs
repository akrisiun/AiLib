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
        public static bool ExecFill<T>(this ISqlProc proc, IList<T> list,
                    IDataMapHelper<T> mapper,
                    Action<double> progress = null) where T : class // , TMap : IDataMapHelper
        {
            Ai.Guard.Check(proc.Connection != null, "proc.Connection null error in ExecFill");
            Ai.Guard.Check(list != null, "list null error in ExecFill");
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


        public static IEnumerable<object[]> ExecEnumerable(this ISqlProc proc, 
                    IDataMapHelper<object[]> mapper,
                    Action<double> progress = null, Action<SqlField[]> onReadFields = null)
        {
            Ai.Guard.Check(proc.Connection != null, "proc.Connection null error in ExecFill");

            using (SqlConnection connection = new SqlConnection(proc.ConnectionString()))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    if (connection.State != ConnectionState.Open)
                        yield break;
                }
                if (connection.Database != proc.DbName)
                    connection.ChangeDatabase(proc.DbName);

                using (var command = proc.CreateCommand())
                {
                    command.Connection = connection;

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

                        if (onReadFields != null)
                            onReadFields(helper.GetFields(dataReader));

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
        public static DbObject Exec(SqlProc proc, Action<double> progress = null, bool withFirst = true)
        {
            var mapper = new DbObject();
            mapper.Worker = DbGetHelper.ExecEnumerable(proc, mapper, progress).GetEnumerator();
            if (mapper.Worker == null)
                return null;
            if (withFirst && mapper.Worker.Current == null)
                mapper.Worker.MoveNext();
            return mapper;
        }

        public Dictionary<string, SqlFieldInfo> Fields { get { return this.fields; } }

        public object MoveNextField(string columnName)
        {
            if (Worker.Current == null && !Worker.MoveNext())
                return null;
            return GetField(columnName, Worker.Current);
        }

        public IEnumerator<object[]> Worker;

        public object[] FirstRecord {
            get
            {
                if (Worker.Current != null)
                    return Worker.Current;
                if (Worker.MoveNext())
                    return Worker.Current;
                return null;
            }
        }
    }

}
