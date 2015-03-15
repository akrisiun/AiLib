using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    public class DbMapperDyn : DbDataMapHelper<ExpandoObject>
    {
        public DbMapperDyn(DbDataReader dataReader)
            : base()
        {
            fields = SqlFieldArray.GetFields(dataReader);
        }

        public ExpandoObject Get(object[] objVal) // IDataRecord record)
        {
            // object[] objVal = DbRecordArray(record.FieldCount);
            // record.GetValues(objVal);

            IDictionary<string, object> val = new ExpandoObject();

            foreach (KeyValuePair<string, SqlFieldInfo> pair in fields)
            {
                object fieldValue = objVal[pair.Value.Ordinal].Equals(DBNull.Value) ? null :
                              objVal[pair.Value.Ordinal];
                val.Add(new KeyValuePair<string, object>(pair.Key, fieldValue));
            }

            return val as ExpandoObject;
        }

        //public static IEnumerable<ExpandoObject> ResultDyn(SqlDataReader reader,
        //    IEnumerator<IDataRecord> numerator)
        //{
        //    if (numerator == null || numerator.Current == null && !numerator.MoveNext())
        //        return Enumerable.Empty<ExpandoObject>(); // yield break;
            
        //    return Entity.ResultDyn.Result(reader, numerator);
        //}

    }

    public class ResultDyn : IEnumerable<ExpandoObject>, IEnumerator<ExpandoObject>, IDisposable
    {
        public static ResultDyn Empty()
        {
            return new ResultDyn() { Reader = null, Numerator = null };
        }

        public SqlDataReader Reader { get; set; }
        public DbEnumeratorData Numerator { get; set; }

        //public DbMapperDyn Helper { get { return helper; } }
        public DbMapperDyn GetHelper() {
            helper = helper ?? new DbMapperDyn(Reader);
            return helper; 
        }
        protected DbMapperDyn helper;

        public void Reset()
        {
            if (Numerator != null && Numerator.Current != null)
            {
                Numerator.Reset();
                Reader = Numerator.Reader;
            }
            if (Reader != null)
                helper = helper ?? new DbMapperDyn(Reader);
        }

        public IEnumerator<ExpandoObject> GetEnumerator()
        {
            Reset();
            DbEnumeratorData numerator = Numerator;
            if (numerator == null || numerator.Reader == null || !numerator.MoveNext())
                yield break;
            
            do
            {
                var rec = numerator.Current as object[]; // IDataRecord;
                if (rec == null || rec.Length == 0)
                    yield break;    // first error
                dynamic obj = helper.Get(rec);
                if (obj != null)
                    yield return obj;
            }
            while (numerator.MoveNext());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Numerator;
        }

        public ExpandoObject Current
        {
            get {
                var rec = Numerator == null ? null : Numerator.Current as object[]; // as IDataRecord;
                return rec == null || helper  == null ? null : helper.Get(rec);
            }
        }

        public void Dispose()
        {
            Reader = null;
            if (Numerator != null)
                Numerator.Dispose();
            Numerator = null;
            helper = null;
        }

        object IEnumerator.Current
        {
            get { return Numerator == null ? null : Numerator.Current; }
        }

        public bool MoveNext()
        {
            if (Numerator == null || !Numerator.MoveNext())
                return false;
            if (helper == null)
            {
                Guard.Check(Reader == Numerator.Reader);
                helper = new DbMapperDyn(Numerator.Reader);
            }
            return helper != null;
        }

    }

}
