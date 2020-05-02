using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;

namespace Dotnet.Entity
{
    public interface IFirstRecord<T> : IEnumerator<T>, // IEnumerable<T>, 
        System.Collections.IEnumerator, IDisposable, ILastError
    {
        int RecordNumber { get; }
        T First { get; }
        bool Prepare();
        bool Any();
    }

    public interface IFirstEnumerable<T> : IEnumerable<T>, IFirstRecord<T>
    {
        ICollection<T> IntoCollection();
    }

    public interface IFirstRecordWrap
    {
        DbDataReader Reader { get; }
        IDbConnection Connection { get; }
        ISqlProc Proc { get; }
    }

    public interface IFirstRecordWrap<T> : IFirstRecordWrap, IEnumerator<T>
    {
        IFirstEnumerable<T> Worker { get; }

        // ExpandoObject
        IDictionary<string, object> Header { get; set; }
    }

    public interface IReaderNextResult
    {
        DbDataReader Reader { get; }
        IDbConnection Connection { get; }

        bool ReaderAvailable { get; }
        Func<Tuple<DbDataReader, IDbConnection>> GetReader { get; } // set; }
        bool NextResult();
    }

}