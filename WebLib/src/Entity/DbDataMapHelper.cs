using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ai.Entity
{
    public interface IDataMapHelper<T>
    {
        IDataMapHelper<T> GetProperties(DbDataReader dataReader);
        T SetValues(object[] objVal);
        object[] DbRecordArray();

        int? GetOrdinal(string columnName);
        object GetField(string columnName, object[] arrayItem);

        Type Type { get; }  // { get { return typeof(T); } }
    }

    public class DbDataMapHelper<T> : IDataMapHelper<T> // where T : class, new()
    {
        public DbDataMapHelper()
        {
            iLen = 0;
            map = null;
            // Type = typeof(T);
            // resultNum = 0;
            properties = null;
        }

        public Type Type
        {
            get { return typeof(T); }
        }

        // private int resultNum;
        private int iLen;
        private int[] map;
        private PropertyInfo[] properties;
        private Dictionary<string, int> Fields;

        public IDataMapHelper<T> GetProperties(DbDataReader dataReader)
        {
            Type type = this.Type;
            iLen = dataReader.FieldCount;
            Ai.Assert.IsTrue(iLen > 0, "DataMapper FieldCount error");

            map = (int[])Array.CreateInstance(typeof(int), iLen);
            Fields = new Dictionary<string, int>();
            for (int j = 0; j < iLen; j++)
                Fields.Add(dataReader.GetName(j), j);

            properties = type.GetProperties(
                         BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < properties.Length; i++)
            {
                for (int j = 0; j < iLen; j++)
                    if (dataReader.GetName(j).Equals(properties[i].Name))
                        map[j] = i;
            }
            return this;
        }

        public int? GetOrdinal(string columnName)
        {
            if (!this.Fields.ContainsKey(columnName))
                return null;
            return this.Fields[columnName];
        }

        public object GetField(string columnName, object[] itemArray)
        {
            int? ordinal = GetOrdinal(columnName);
            return ordinal.HasValue ? 
                   (itemArray[ordinal.Value] == DBNull.Value ? null : itemArray[ordinal.Value])
                   : null;
        }

        public T SetValues(object[] objVal)
        {
            // object[] : {"No parameterless constructor defined for this object."}
            if (Type.IsArray)
            {
                T valArray = (T)objVal.Clone(); //  Activator.CreateInstance(typeof(T), objVal.Length);
                // default(T) : non-static method requires a target
                // object[] array = valArray as object[];
                return valArray;
            }
            T val = Activator.CreateInstance<T>();

            for (int i = 0; i < map.Length; i++)
                if (i == 0 || map[i] > 0)
                {
                    PropertyInfo info = properties[map[i]];
                    if (!objVal[i].Equals(DBNull.Value))
                        info.SetValue(val, objVal[i], null);
                }

            return val;
        }

        public object[] DbRecordArray()
        {
            return (object[])Array.CreateInstance(typeof(object), iLen);
        }

    }

}
