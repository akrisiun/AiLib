using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ai.Util
{
    public class SqlDataRowInfo
    {
        public readonly DataRow row;

        public SqlDataRowInfo(DataRow row)
        { this.row = row; }

        public override string ToString()
        {
            if (row == null) return "null";
            return "[Row #" + row.Table.Rows.IndexOf(row).ToString() + " Columns=" + row.Table.Columns.Count.ToString() + "]";
        }

        // Debuging ItemArray
        public IDictionary<string, ArrayObject> ItemArrayList
        {
            get
            {
                IDictionary<string, ArrayObject> list = new System.Collections.Generic.Dictionary<string, ArrayObject>();
                for (int ordinal = 0; ordinal < row.ItemArray.Length; ordinal++)
                {
                    var value = row.ItemArray.GetValue(ordinal);
                    list.Add(row.Table.Columns[ordinal].ColumnName
                        , new ArrayObject(new object[]
                            { value == null ? "null" : value.ToString(), 
                              value.GetType().ToString().Replace("System.", "") 
                            }));
                }
                return list;
            }
        }
    }

    public class ArrayObject
    {
        protected object[] array;

        public ArrayObject(object[] array)
        {
            this.array = array;
        }

        public override string ToString()
        {
            if (array.Length == 1) return array[0].ToString();
            var str = " " + (array[0] == null ? "null" : array[0].ToString())
                    + "| " + (array[1] == null ? "null" : array[1].ToString());
            if (array.Length > 3)
                str += "| " + array[2].ToString()
                       + ((array.Length < 3) ? "" : "| "
                            + (array[3].ToString()
                              + ((array.Length < 4) ? "" : "| " + array[4].ToString()))
                         ) + " |";
            return str;
        }

    }

}
