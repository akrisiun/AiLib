using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    public class SqlTableMapper : IDataMapHelper<object[]>
    {
        public SqlTableMapper(Action<SqlTableMapper, DbDataReader> propertiesParser)
        {
            this.propertiesParser = propertiesParser;
        }
        protected Action<SqlTableMapper, DbDataReader> propertiesParser;

        public Type Type { get { return typeof(object[]); } }
        public int FieldCount { get { return iLen; } }

        int iLen;
        string[] FieldNames;

        IDataMapHelper<object[]> IDataMapHelper<object[]>.GetProperties(DbDataReader dataReader)
        {
            iLen = dataReader.FieldCount;
            FieldNames = new string[iLen];
            for (int i = 0; i < iLen; i++)
                FieldNames[i] = dataReader.GetName(i);

            if (propertiesParser != null)
                propertiesParser(this, dataReader);
            return this;
        }

        public object[] SetValues(object[] objVal)
        {
            return objVal;
        }

        public object[] DbRecordArray()
        {
            return (object[])Array.CreateInstance(typeof(object), iLen);
        }

        public object[] DbRecordArray(int len)
        {
            return (object[])Array.CreateInstance(typeof(object), len);
        }

        public virtual int? GetOrdinal(string columnName)
        {
            for (int i = 0; i < FieldNames.Length; i++)
                if (FieldNames[i] == columnName)
                    return i;
            return null;
        }

        public virtual object GetField(string columnName, object[] arrayItem)
        {
            int? find = GetOrdinal(columnName);
            return find.HasValue ? arrayItem[find.Value] != DBNull.Value ? arrayItem[find.Value] : null
                                 : null;
        }

        public virtual SqlField[] GetFields(DbDataReader dataReader)
        {
            return null;
        }

    }

}
