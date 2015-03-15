// Assembly location: C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\Profile\Client\System.Data.DataSetExtensions.dll
// https://raw.githubusercontent.com/Microsoft/referencesource/master/System.Data.DataSetExtensions/System/Data/DataRowExtensions.cs

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    /// <summary>
    /// This static class defines the DataRow extension methods.
    /// </summary>
    public static class DataRowExtensions
    {

        public static T Field<T>(this DataRow row, string columnName)
        {
            Guard.CheckArgumentNull(row, "row");
            return UnboxT<T>.Unbox(row[columnName]);
        }

        public static T Field<T>(this DataRow row, DataColumn column)
        {
            Guard.CheckArgumentNull(row, "row");
            return UnboxT<T>.Unbox(row[column]);
        }

        public static T Field<T>(this DataRow row, int columnIndex)
        {
            Guard.CheckArgumentNull(row, "row");
            return UnboxT<T>.Unbox(row[columnIndex]);
        }

        public static T Field<T>(this DataRow row, int columnIndex, DataRowVersion version)
        {
            Guard.CheckArgumentNull(row, "row");
            return UnboxT<T>.Unbox(row[columnIndex, version]);
        }

        public static T Field<T>(this DataRow row, string columnName, DataRowVersion version)
        {
            Guard.CheckArgumentNull(row, "row");
            return UnboxT<T>.Unbox(row[columnName, version]);
        }

        public static T Field<T>(this DataRow row, DataColumn column, DataRowVersion version)
        {
            Guard.CheckArgumentNull(row, "row");
            return UnboxT<T>.Unbox(row[column, version]);
        }

        public static void SetField<T>(this DataRow row, int columnIndex, T value)
        {
            Guard.CheckArgumentNull(row, "row");
            row[columnIndex] = (object)value ?? DBNull.Value;
        }

        public static void SetField<T>(this DataRow row, string columnName, T value)
        {
            Guard.CheckArgumentNull(row, "row");
            row[columnName] = (object)value ?? DBNull.Value;
        }

        public static void SetField<T>(this DataRow row, DataColumn column, T value)
        {
            Guard.CheckArgumentNull(row, "row");
            row[column] = (object)value ?? DBNull.Value;
        }

        private static class UnboxT<T>
        {
            internal static readonly Converter<object, T> Unbox = Create(typeof(T));

            private static Converter<object, T> Create(Type type)
            {
                if (type.IsValueType)
                {
                    if (type.IsGenericType && !type.IsGenericTypeDefinition
                        && (typeof(Nullable<>) == type.GetGenericTypeDefinition()))
                    {
                        return (Converter<object, T>)Delegate.CreateDelegate(
                            typeof(Converter<object, T>),
                                typeof(UnboxT<T>)
                                    .GetMethod("NullableField",
                                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
                                    )
                                    .MakeGenericMethod(type.GetGenericArguments()[0]));
                    }
                    return ValueField;
                }
                return ReferenceField;
            }

            private static T ReferenceField(object value)
            {
                return ((DBNull.Value == value) ? default(T) : (T)value);
            }

            private static T ValueField(object value)
            {
                if (DBNull.Value == value)
                {
                    // throw DataSetUtil.InvalidCast(Strings.DataSetLinq_NonNullableCast(typeof(T).ToString()));
                    throw new InvalidOperationException("Error of NonNullableCast " + typeof(T).ToString());
                }
                return (T)value;
            }

            private static Nullable<TElem> NullableField<TElem>(object value) where TElem : struct
            {
                if (DBNull.Value == value)
                {
                    return default(Nullable<TElem>);
                }
                return new Nullable<TElem>((TElem)value);
            }
        }
    }
}
