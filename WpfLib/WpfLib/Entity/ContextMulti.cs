using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace Ai.Entity
{
    public class MultiResult<T> : IDisposable where T : class
    {
        #region Properties, Data
        public int Count { get; private set; }
        public int Depth { get { return reader == null ? -1 : reader.Depth; } }
        public SqlDataReader Reader { get { return reader; } }
        public ISqlProc Proc { get { return proc; } }
        public Dictionary<string, SqlFieldInfo> Fields { get; private set; }
        public object[] FirstRecord { get; private set; }
        public IEnumerator Numerator { get { return numerator; } }

        public MultiResult(ISqlProc proc = null)
        {
            Count = 1;
            // mapper = null;
            this.proc = proc;
            reader = null;
            mapHelper = null;
        }

        ~MultiResult()
        {
            (this as IDisposable).Dispose();
        }
        void IDisposable.Dispose()
        {
            if (reader != null && !reader.IsClosed)
                reader.Dispose();
            reader = null;
            // mapper = null;
            if (proc != null && proc.Connection != null)
                proc.Connection.Close();
            proc = null;
            mapHelper = null;
            numerator = null;
        }

        ISqlProc proc;
        IDataMapHelper<object[]> mapHelper;     // DbObject mapper;
        SqlDataReader reader;
        DbEnumeratorData numerator;

        #endregion

        public MultiResult<T> Prepare(SqlProc proc
                , Action<SqlCommand> setup = null
                , Action<SqlField[]> onReadFields = null
                , bool noMoveFirst = false
                )
        {
            this.proc = proc;
            var mapper = new DbObject();

            numerator = DbEnumeratorData.GetEnumerator(() =>
                {
                    this.reader = SqlMultiDyn.ExecMultiReader(proc, setup, progress: null);
                    return reader;
                });

            if (numerator == null)
                return null;
            if (reader.IsClosed || reader.Depth != 0)
                return null;

            if (noMoveFirst)
                return this;

            numerator.MoveNext();
            var rec = numerator.Current as object[]; // DbDataRecord;
            mapHelper = mapper.GetProperties(reader);

            Fields = reader.GetFields();    // SqlFieldArray
            FirstRecord = rec;
            // mapHelper.SetValues(mapHelper.DbRecordArray());
            return this;
        }

        public static object[] DbRecord(int iLen)
        {
            return (object[])Array.CreateInstance(typeof(object), iLen);
        }

        public IEnumerable<object[]> ResultObj()
        {
            var cycle = numerator;
            do
            {
                var rec = numerator.Current as object[]; // DbDataRecord;
                if (rec == null)
                    yield break;    // first error
                yield return rec;
            } while (cycle.MoveNext());
        }

        public IEnumerable<ExpandoObject> ResultDyn()
        {
            var cycle = numerator;
            var helper = new DbMapperDyn(reader);
            do
            {
                object[] rec = numerator.Current; // as DbDataRecord;
                if (rec == null)
                    yield break;    // first error
                dynamic obj = helper.Get(rec);
                yield return obj;
            } while (cycle.MoveNext());
        }

        public IEnumerable<TRes> Result<TRes>() where TRes : class
        {
            Ai.Guard.Check(proc != null && proc.Connection != null, "MultiResult proc error");
            if (numerator == null)
                yield break;
            if (typeof(TRes).Equals(typeof(object[])))
            {
                do
                {
                    var values = numerator.Current as object[]; // DbDataRecord;
                    yield return values as TRes;
                } while (numerator.MoveNext());
            }

            var helper = new DbDataMapHelper<TRes>();

            var cycle = numerator;
            do
            {
                var rec = numerator.Current as object[]; // as DbDataRecord;
                if (rec == null)
                    yield break;    // first error

                var objArray = DbRecord(reader.FieldCount);
                TRes obj = helper.SetValues(objArray);
                yield return obj;
            }
            while (cycle.MoveNext());
        }

        public bool NextResult() //  Next<T>(DbDataMapHelper<T> helper)
        {
            if (Depth < 0 || numerator == null)
                return false;
            if (!numerator.NextResult())
                return false;
            return numerator.ReaderAvailable;
        }

    }

    public static class ContextMulti
    {
        public static MultiResult<T> MultiExec<T>(this Context db, SqlCommand cmd) where T : class
        {
            var proc = new SqlProc
            {
                Connection = new SqlConnection(db.ConnectionString()),
                CmdText = cmd.CommandText,
                Param = SqlProc.CloneParam(cmd.Parameters)
            };

            return MultiExec<T>(proc);
        }

        public static MultiResult<T> MultiExec<T>(this Context db, object sqlProcNamed) where T : class
        {
            var proc = SqlProcExt.ProcNamed(sqlProcNamed);
            proc.Context = db;
            return MultiExec<T>(proc);
        }

        public static KeyValuePair<SqlDataReader, IEnumerable<T>>
            ExecFirst<T>(this Context db, object sqlProcNamed,
                Action<Exception> onError = null
                ) where T : class
        {
            var proc = SqlProcExt.ProcNamed(sqlProcNamed);
            proc.Context = db;
            SqlDataReader reader = null;
            var numerator = ContextMulti.MultiExec<T>(proc, 
                    onReadFields: (r, fields) =>
                    {
                        reader = r;
                    }, onError: onError);

            if (reader == null)
            {
                var num = numerator.GetEnumerator();
                num.MoveNext();
            }

            if (numerator == null)
                numerator = Enumerable.Empty<T>();

            return new KeyValuePair<SqlDataReader, IEnumerable<T>>(reader, numerator);
        }

        public static MultiResult<T> MultiExec<T>(this SqlProc proc) where T : class
        {
            MultiResult<T> result = new MultiResult<T>();
            result.Prepare(proc);
            return result;
        }

        public static MultiResult<object[]> MultiObj(this Context db, string proc, Action<SqlCommand> setup = null)
        {
            var procMap = new SqlProc { Context = db, Connection = db.SqlConnection, CmdText = proc, Param = null };
            var result = new MultiResult<object[]>();
            return result.Prepare(procMap, setup);
        }

        public static MultiResult<object[]> MultiObj(this SqlProc proc)
        {
            var result = new MultiResult<object[]>();
            return result.Prepare(proc);
        }

        public static IEnumerable<object[]> ResultObj(this SqlProc proc
                , Action<SqlField[]> onReadFields = null)
        {
            var cmd = proc.CreateCommand();
            var helper = new DbDataMapHelper<object[]>();
            return DbGetHelper.ExecEnumerable(proc, helper, onReadFields: onReadFields);
        }

        public static IEnumerable<T> MultiExec<T>(this SqlProc proc,
            Action<SqlDataReader, SqlField[]> onReadFields = null,
            Action<Exception> onError = null)
            where T : class
        {
            var cmd = proc.CreateCommand();
            var mapper = new DbObject();

            SqlDataReader reader = SqlMultiDyn.ExecMultiReader(proc, onError: onError);
            if (reader == null)
                yield break;

            IEnumerable<object[]> worker = ExecMulti(proc, reader, mapper, onReadFields, progress: null);
            var numerator = worker.GetEnumerator();

            Guard.Check(numerator.Current == null);
            while (numerator.MoveNext())
            {
                object[] record = numerator.Current;
                T obj = (T)Activator.CreateInstance<T>();

                yield return obj;
            }

        }

        public static IEnumerable<object[]> ExecMulti(this ISqlProc proc, SqlDataReader dataReader,
                    IDataMapHelper<object[]> mapper,
                    Action<SqlDataReader, SqlField[]> onReadFields = null,
                    Action<double> progress = null)
        {
            var helper = mapper;
            helper.GetProperties(dataReader);

            if (onReadFields != null)
                onReadFields(dataReader, helper.GetFields(dataReader));

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
