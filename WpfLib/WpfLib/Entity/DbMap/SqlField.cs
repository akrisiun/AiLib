using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Ai.Entity
{

    public class SqlField
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public Type Type { get; set; }
        public int Ordinal { get; set; }
        public int OrdinalVisible { get; set; }

        public int? Width { get; set; }
        public int? MaxLength { get; set; }
        public bool Nullable { get; set; }

        public string CssClass { get; set; }
        public bool WordWrap { get; set; }
        public bool Hide { get; set; }

        public override string ToString()
        {
            return Ordinal + ":" + Name + " " + Type.FullName;
        } 
    }

    public static class SqlFieldArray
    {
        public static SqlField[] GetArray(this DbDataReader dbReader)
        {
            var fld = new SqlField[] {};
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
            }
            return fld;
        }

        public static Dictionary<string, SqlFieldInfo> GetFields(this DbDataReader dataReader)  // SqlDataReader dataReader)
        {
            var fields = new Dictionary<string, SqlFieldInfo>();
            int iLen = dataReader == null ? 0 : dataReader.FieldCount;
            for (int j = 0; j < iLen; j++)
            {
                fields.Add(dataReader.GetName(j),
                    new SqlFieldInfo() { Ordinal = j,
                        SqlType = dataReader.GetProviderSpecificFieldType(j) });
            }
            return fields;
        }
    }

}
