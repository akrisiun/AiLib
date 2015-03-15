using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ai.Entity
{
    public interface IDataMapHelper<T>
    {
        IDataMapHelper<T> GetProperties(DbDataReader dataReader);
        SqlField[] GetFields(DbDataReader dataReader);

        T SetValues(object[] objVal);
        object[] DbRecordArray();
        object[] DbRecordArray(int len);

        int? GetOrdinal(string columnName);
        object GetField(string columnName, object[] arrayItem);

        Type Type { get; }  // { get { return typeof(T); } }
    }

}
