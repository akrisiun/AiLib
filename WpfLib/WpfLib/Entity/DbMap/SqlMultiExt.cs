using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    public static class SqlMultiExt
    {
        public static Func<SqlDataReader> LazyReader(ISqlProc proc
                , Action<SqlDataReader> readerGet
                , Action<SqlCommand> setupCmd = null
                , Action<Exception> onError = null)
        {
            return SqlMultiDyn.LazyReader(proc, readerGet, setupCmd, onError);
        }

        public static ResultDyn ExecNamedResultDyn(this Context db, object named
                , Action<SqlCommand> setupCmd = null
                , Action<Exception> onError = null)
        {
            var proc = SqlProcExt.ProcNamed(named, db);
            SqlDataReader reader = null;
            return DbEnumeratorData.GetResultDyn(LazyReader(proc, (r) => reader = r, setupCmd, onError));
        }

        public static ResultDyn ExecProcResultDyn(this ISqlProc proc
                , Action<SqlCommand> setupCmd = null
                , Action<Exception> onError = null)
        {
            SqlDataReader reader = null;
            return DbEnumeratorData.GetResultDyn(SqlMultiDyn.LazyReader(proc, (r) => reader = r, setupCmd, onError));
        }

        public static KeyValuePair<ResultDyn, IEnumerable<T>> ResultObj<T>(this Context db, object named
                , Action<SqlCommand> setupCmd = null
                , Action<Exception> onError = null)
        {
            var proc = SqlProcExt.ProcNamed(named);
            SqlDataReader reader = null;
            ResultDyn dyn = DbEnumeratorData.GetResultDyn(LazyReader(proc, (r) => reader = r, setupCmd, onError));
            return CastResult<T>(dyn, onError);
        }

        public static KeyValuePair<ResultDyn, IEnumerable<T>> CastResult<T>(this ResultDyn dyn
                , Action<Exception> onError = null)
        {
            return new KeyValuePair<ResultDyn, IEnumerable<T>>(dyn, dyn.Cast<T>());
        }

    }
}
