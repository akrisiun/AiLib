using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ai.Entity.DataTables
{
    public static class SqlTableData
    {
        public static bool Exec(this SqlObjTable tableObj, DbContext context,
               string[] param, int iFrom, int iTake)
        {
            string execName = param[0];

            var proc = SqlProcExt.CmdText(execName, context);

            var fld = tableObj.Fields;

            DataTable table = proc.Result();

            #region Head
            Array.Resize<SqlField>(ref fld, table.Columns.Count);
            tableObj.Fields = fld;

            foreach (DataColumn column in table.Columns)
            {
                int ordinal = column.Ordinal;
                tableObj.Fields[ordinal] = new SqlField()
                {
                    Ordinal = ordinal,
                    Name = column.ColumnName,
                    Caption = column.Caption,
                    Type = column.DataType,
                    Width = 0
                };
            }

            #endregion

            var query = table.Rows.Cast<DataRow>().AsQueryable<DataRow>();
            if (iTake > 0)
                query = query.Take(iTake);
            if (iFrom > 0)
                query = query.Skip(iFrom);

            foreach (DataRow row in query)
                table.Rows.Add(row.ItemArray);

            return true;
        }
    }
}
