using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace Dotnet.Entity
{

    [StructLayout(LayoutKind.Sequential)]
    public struct SqlField
    {
        public static string NoData { get { return "No data"; } }

        public string Name { get; set; }
        public string Caption { get; set; }
        public Type Type { get; set; }
        public int Ordinal { get; set; }
        public int OrdinalVisible { get; set; }

        public int? Width { get; set; }
        public int? MaxLength { get; set; }

        // Sql variables
        public bool Nullable { get; set; }  // IsNull at SqlFieldInfo
        public string SqlTypeName { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }

        public string CssClass { get; set; }
        public bool WordWrap { get; set; }
        public bool Hide { get; set; }

        public override string ToString()
        {
            return String.Format("{0}: {1} {2}", Ordinal, Name, Type.FullName);
        }

        public string ToSqlString()
        {
            var SqlType = Type;
            if (SqlTypeName == null && SqlType != null)
                SqlTypeName = SqlType.ToString().Replace("System.", "").ToLower();

            return String.Format("{0}{1}{2}",
                SqlTypeName,
                MaxLength == null || MaxLength == 0 || IsInt && MaxLength <= 4 || IsXml || IsText || IsDate
                    ? null
                    : (NumericScale > 0 && IsFloat
                        ? String.Format("({0},{1})", NumericPrecision, NumericScale)
                        : String.Format("({0})", MaxLength)),
                Nullable ? " NULL" : string.Empty);
        }

        public bool IsInt { get { return SqlTypeName != null && SqlTypeName.IndexOf("int", 0, StringComparison.CurrentCultureIgnoreCase) >= 0; } }
        public bool IsXml { get { return SqlTypeName != null && SqlTypeName.IndexOf("xml", 0, StringComparison.CurrentCultureIgnoreCase) >= 0; } }
        public bool IsText { get { return SqlTypeName != null && SqlTypeName.IndexOf("text", 0, StringComparison.CurrentCultureIgnoreCase) >= 0; } }
        public bool IsDate { get { return SqlTypeName != null && SqlTypeName.IndexOf("date", 0, StringComparison.CurrentCultureIgnoreCase) >= 0; } }
        public bool IsFloat
        {
            get
            {
                return SqlTypeName != null
                    && (SqlTypeName.IndexOf("decimal", 0, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || SqlTypeName.IndexOf("float", 0, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || SqlTypeName.IndexOf("money", 0, StringComparison.CurrentCultureIgnoreCase) >= 0
                    );
            }
        }

        public static SqlField[] GetArray(DbDataReader dbReader) { return SqlFieldArray.GetArray(dbReader); }
    }

    public static class SqlFieldArray
    {

        public static SqlField[] GetArray(this IDataReader dbReader)
        {
            if (dbReader == null || !(dbReader is DbDataReader))
                return null;

            return GetArray(dbReader as DbDataReader);
        }

        public static SqlField[] GetArray(this DbDataReader dbReader)
        {
            var fld = new SqlField[] { };
            if (dbReader == null || dbReader.IsClosed || dbReader.FieldCount <= 0)
                return fld;

            int len = dbReader.FieldCount;
            Array.Resize<SqlField>(ref fld, len);
            for (int i = 0; i < len; i++)
            {
                var typ = dbReader.GetFieldType(i);
                fld[i] = new SqlField()
                {
                    Ordinal = i,
                    Name = dbReader.GetName(i),
                    Type = typ,
                    Caption = dbReader.GetName(i),
                    Hide = false
                };

                // http://stackoverflow.com/questions/374651/how-to-check-if-an-object-is-nullable
                fld[i].Nullable = Nullable.GetUnderlyingType(typ) != null; // Nullable<T>
                fld[i].OrdinalVisible = i;
            }
            return fld;
        }

        public static Dictionary<string, SqlFieldInfo> GetFields(this DbDataReader dataReader, 
            Action<Exception> onDublicateField = null)
        {
            var fields = new Dictionary<string, SqlFieldInfo>();
            int iLen = dataReader == null || dataReader.IsClosed ? 0 : dataReader.FieldCount;

            if (onDublicateField != null)
            {
                for (int j = 0; j < iLen; j++)
                {
                    var name = dataReader.GetName(j);
                    try
                    {

                        fields.Add(name,
                            new SqlFieldInfo()
                            {
                                Ordinal = j,
                                SqlType = dataReader.GetProviderSpecificFieldType(j)
                            });
                    }
                    catch (Exception ex)
                    {
                        onDublicateField(
                             new Exception(String.Format("Dublicate field #{0}, {1}", j, name), ex)
                         );
                    }
                }
            }
            else
                for (int j = 0; j < iLen; j++)
                {
                    var name = dataReader.GetName(j);

                    if (!fields.ContainsKey(name))
                        fields.Add(name,
                            new SqlFieldInfo()
                            {
                                Ordinal = j,
                                SqlType = dataReader.GetProviderSpecificFieldType(j)
                            });
                }
            return fields;
        }
    }

}
