using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Reflection;
using System.Text;

namespace Ai.Entity
{
    public struct SqlFieldInfo
    {
        public int Ordinal;
        public Type SqlType;
    }

    public class DbDataMapHelper<T> : IDataMapHelper<T> // where T : class, new()
    {
        public DbDataMapHelper()
        {
            iLen = 0;
            map = null;
            properties = null;
        }

        public Type Type
        {
            get { return typeof(T); }
        }

        protected int iLen;
        protected int[] map;
        protected PropertyInfo[] properties;
        protected Dictionary<string, SqlFieldInfo> fields;

        public IDataMapHelper<T> GetProperties(DbDataReader dataReader)
        {
            Type type = this.Type;
            if (dataReader.IsClosed)
                return null;

            iLen = dataReader.FieldCount;
            Ai.Guard.Check(iLen > 0, "DataMapper FieldCount error");

            map = (int[])Array.CreateInstance(typeof(int), iLen);
            fields = SqlFieldArray.GetFields(dataReader);

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
            if (!this.fields.ContainsKey(columnName))
                return null;
            return this.fields[columnName].Ordinal;
        }

        public object GetField(string columnName, object[] itemArray)
        {
            int? ordinal = GetOrdinal(columnName);
            return ordinal.HasValue ? 
                   (itemArray[ordinal.Value] == DBNull.Value ? null : itemArray[ordinal.Value])
                   : null;
        }

        public virtual T SetValues(object[] objVal)
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

        public object[] DbRecordArray(int iLen)
        {
            return (object[])Array.CreateInstance(typeof(object), iLen);
        }

        public virtual SqlField[] GetFields(DbDataReader dataReader)
        {
            return SqlFieldArray.GetArray(dataReader);
        }

    }

}
