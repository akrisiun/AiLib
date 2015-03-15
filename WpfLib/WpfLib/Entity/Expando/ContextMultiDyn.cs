using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    using ReaderEnumExpando = KeyValuePair<SqlDataReader, IEnumerator<ExpandoObject>>;

    public static class ContextMergetDyn
    {
        //public static IEnumerable<ExpandoObject>
        //    ExecMergeDyn(this Context db, object namedParam
        //        , Action<SqlCommand> setupCmd = null)
        //{
        //    ReaderEnumExpando firstSet = db.ExecDyn(namedParam, setupCmd);
        //    var reader = firstSet.Key;
        //    var numerator = firstSet.Value;
        //    if (numerator.Current != null)
        //        yield return numerator.Current;     // first record already loaded

        //    while (numerator.MoveNext())
        //        yield return numerator.Current;

        //    numerator.Dispose();

        //    while (!reader.IsClosed)
        //    {
        //        if (!reader.NextResult())
        //            yield break;

        //        var result2 = DbEnumeratorData.ResultDyn(reader);
        //        while (result2.MoveNext())
        //            yield return result2.Current;

        //        result2.Dispose();
        //    }
        //}
    }

    public static class ContextMultiDyn
    {
        public static KeyValuePair<SqlDataReader, DbEnumeratorData<ExpandoObject>>
            ExecDyn(this Context db, object namedParam
                , Action<SqlCommand> setupCmd = null)
        {
            var proc = SqlProcExt.ProcNamed(namedParam);
            proc.Context = db;

            SqlDataReader readerGet = null;
            var numerator 
                = SqlMultiDyn.ResultDyn(proc, (reader) => readerGet = reader, setupCmd);
            if (numerator.Current == null || readerGet == null)
                numerator.MoveNext();

            return new KeyValuePair<SqlDataReader, DbEnumeratorData<ExpandoObject>>(readerGet, numerator);
        }
       
    }
 
}
