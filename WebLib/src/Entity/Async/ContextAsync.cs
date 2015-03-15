using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Ai.Entity
{
    public static class ContextAsync
    {
        public static Task<IList<T>> ExecAsync<T>(this Context context, SqlProc procedure, IList<T> list,
            TaskScheduler scheduler, Action<double> progress) where T : class
        {
            Func<IList<T>> func = () =>
            {
                var conn = new SqlConnection();
                conn.ConnectionString = context.ConnectionString();
                if (context.OnBeforeOpen != null)
                    context.OnBeforeOpen(context, new SqlConnEventArgs(conn));
                conn.Open();

                var procClone = new SqlProc() { CmdText = procedure.CmdText, Connection = conn, Param = procedure.Param };

                Trace.WriteLine("SqlProc: " + procClone.CmdText);
                var res = procClone.Exec<T>(list, progress); // .Result;
                return res;
            };

            var task = Task.Factory.StartNew<IList<T>>(func, CancellationToken.None, TaskCreationOptions.LongRunning, scheduler);

            return task;
        }

        public static async Task<object> OpenAsync(Context context, SqlConnection sqlConn)  // , CancellationToken cancellationToken)
        {
            if (sqlConn.State == ConnectionState.Open)
                return true;

            Exception error;
            try
            {
                return await Task.Factory.StartNew(() =>
                    {
                        if (context.OnBeforeOpen != null)
                            context.OnBeforeOpen(context, new SqlConnEventArgs(sqlConn));
                        sqlConn.Open();
                        return sqlConn.State == ConnectionState.Open;
                    });
            }
            catch (Exception e)
            {
                error = e;
                // result.SetException(e);
            }

            var errTask = new TaskCompletionSource<object>();
            errTask.SetException(error);
            return errTask.Task;
        }

        public static async Task<DataTable> ExecuteAsync(this Context context
                , SqlProc procedure, bool parseError = true)
        {
            if (procedure.Connection == null)
            {
                procedure.Connection = context.SqlConnection;
                if (procedure.Connection == null)
                    return null;
            }

            Trace.WriteLine("SqlProc: " + procedure.CmdText);

            if (procedure.Connection.State != ConnectionState.Open)
            {
                Task<object> task = OpenAsync(context, procedure.Connection); // , CancellationToken.None);
                await task;
                if (task.Exception != null)
                {
                    var tcs = new TaskCompletionSource<DataTable>();
                    tcs.SetException(task.Exception);
                    context.LastError = task.Exception;
                    if (!parseError)
                        throw task.Exception;

                    return await tcs.Task;
                }
            }

            if (!parseError)
                return await Task.Factory.StartNew(() =>
                {
                    return procedure.Result();
                });

            try
            {
                return await Task.Factory.StartNew(() =>
                {
                    return procedure.Result();
                });
            }
            catch (Exception ex)
            { context.LastError = ex; }
            return null;
        }

    }
}
