using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ai.Entity
{
    public interface IFirstRecord<T> : IEnumerator<T>, IEnumerable<T>, System.Collections.IEnumerator, IDisposable
    {
        int RecordIndex { get; }
        T First { get; }
    }

    public interface IReaderNextResult
    {
        SqlDataReader Reader { get; }
        bool ReaderAvailable { get; }
        Func<SqlDataReader> GetReader { get; set; }
        bool NextResult();
    }

    public class DbEnumeratorData<T> : DbEnumeratorData, IEnumerator<T>, IEnumerable<T>, System.Collections.IEnumerator
    {
        public static new DbEnumeratorData<T> Empty { get { return (DbEnumeratorData<T>)(object)DbEnumeratorData.Empty; } }

        public DbMapperDyn Helper { get; private set; }

        public DbEnumeratorData(Func<SqlDataReader> getReader)
            : base(getReader)
        {
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            if (Current == null)
                Helper = null;
        }

        public override bool NextResult()
        {
            Helper = null;
            if (!base.NextResult())
                return false;
            if (ReaderAvailable && Current != null)
            {
                Helper = null;
                Helper = new DbMapperDyn(Reader);
            }
            return ReaderAvailable;
        }

        public new T Current
        {
            get
            {
                // IDataRecord 
                object[] obj = base.Current;
                if (obj == null || Helper == null)
                    return default(T);

                var helper = Helper;
                ExpandoObject expando = helper == null ? null : helper.Get(obj);
                if (expando == null)
                    return default(T);
                if (typeof(T) == typeof(ExpandoObject))
                    return (T)(object)expando;

                T res = (T)Convert.ChangeType(expando, typeof(T));
                return res;
            }
        }

        public new T First
        {
            get {
                object[] obj =  base.first;
                if (obj == null || obj.Length <= 0 || Helper == null)
                    return default(T);

                var helper = Helper;
                ExpandoObject expando = helper == null ? null : helper.Get(obj);
                if (expando == null)
                    return default(T);
                if (typeof(T) == typeof(ExpandoObject))
                    return (T)(object)expando;

                T res = (T)Convert.ChangeType(expando, typeof(T));
                return res;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return base.Current; }
        }

        public override bool MoveNext()
        {
            if (!base.MoveNext())
                return false;
            if (Helper == null && ReaderAvailable)
                Helper = new DbMapperDyn(Reader);
            return true;
        }

        public new IEnumerator<T> GetEnumerator()
        {
            if (Current != null && resultNum > 0)  // || base.Base != null 
                Reset();
            return this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

      
    }

    public class DbEnumeratorData : IFirstRecord<object[]>, IReaderNextResult   // IDataRecord
    {
        #region Static
        static DbEnumeratorData()
        {
            Empty = new DbEnumeratorData(null);
        }
        public static DbEnumeratorData Empty { get; private set; }
        public static DbEnumeratorData
               GetEnumerator(Func<SqlDataReader> getReader)
        {
            var numerator = new DbEnumeratorData(getReader);
            numerator.iRecord = -1;
            return numerator;
        }

        public static // IEnumerator<ExpandoObject> 
            ResultDyn GetResultDyn(Func<SqlDataReader> getReader)
        {
            var numerator = GetEnumerator(getReader);
            if (numerator == null || numerator.Reader == null)
                return ResultDyn.Empty();

            var dynNumerator = new ResultDyn { Reader = numerator.Reader, Numerator = numerator };
            return dynNumerator;
        }

        //public static bool NextResult(SqlDataReader reader)
        //{
        //    if (reader == null || reader.IsClosed)
        //        return false;
        //    return reader.NextResult();
        //}

        #endregion
        #region Properties
        public object[] First { get { return first; } }
        public int RecordIndex { get { return iRecord; } }

        // protected DbEnumerator Base { get; private set; }

        public DbEnumeratorData() { iRecord = -1; resultNum = -1; }
        public DbEnumeratorData(Func<SqlDataReader> getReader)
        {
            first = null;
            iRecord = -1;
            resultNum = -1;
            GetReader = getReader;
            if (GetReader != null)
                Reset();
        }

        public Func<SqlDataReader> GetReader { get; set; }
        public SqlDataReader Reader { get; private set; }
        public bool ReaderAvailable
        {
            get
            {
                var reader = Reader;
                return reader != null && !reader.IsClosed && reader.FieldCount > 0;
            }
        }

        public virtual bool NextResult()
        {
            if (!ReaderAvailable)
                return false;
            var reader = Reader;
            if (reader == null || reader.IsClosed || !reader.NextResult())
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                return false;
            }

            // Base = null;
            resultNum++;
            first = null;
            Reset();

            if (!ReaderAvailable)
                return false;
            if (First == null)
            {
                MoveNext();
                Reset();
            }
            return First != null;
        }

        public object[] Current
        {
            get
            {
                if (iRecord < 0 || !ReaderAvailable || !Reader.HasRows) // .RecordsAffected < 0)
                    return null;
                if (first != null && iRecord == 0 && first.Length == Reader.FieldCount)
                    return first;

                var reader = Reader;
                object[] values = (object[])Array.CreateInstance(typeof(object), reader.FieldCount);
                reader.GetValues(values);
                return values;
            }
        }

        protected int iRecord;
        protected int resultNum;
        protected object[] first;   // IDataRecord

        #endregion

        public virtual void Reset()
        {
            if (Reader != null && iRecord < 0 // && Base != null
                || iRecord == 0 && first != null
                   && first.Length == Reader.FieldCount)
                return;

            // Base.Reset();   // exception
            if (Reader != null || resultNum <= 0)
            {
                try
                {
                    if (Reader != null)
                        Reader.Dispose();           // Timeout
                }
                catch { }
                Reader = null;
                Reader = GetReader();
            }

            if (resultNum < 0)
                resultNum = 0;
            if (Reader != null || resultNum == 0)
            {
                iRecord = -1;
                first = null;
            }
        }

        public virtual bool MoveNext()
        {
            if (iRecord == 0 && first != null && first.Length == Reader.FieldCount && Reader.HasRows)
            {
                // after Reset
                iRecord++;
                return true;    // already got first record
            }

            iRecord++;
            if (!ReaderAvailable || !Reader.Read()) // Base == null || !Base.MoveNext())
            {
                iRecord = -1;
                return false;
            }

            if (iRecord == 0)
                first = Current;
            return true;
        }

        public void Dispose()
        {
            // cashe = null;
            // Base = null;
            Reader = null;
            first = null;
        }

        #region Implement
        object System.Collections.IEnumerator.Current { get { return Current; } }

        bool System.Collections.IEnumerator.MoveNext()
        {
            return MoveNext();
        }

        void System.Collections.IEnumerator.Reset()
        {
            Reset();
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this;
        }
        #endregion

        //IEnumerator<object[]> IEnumerable<object[]>.GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}
    }

}
