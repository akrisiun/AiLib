using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO = System.IO;


namespace Ai.Excel
{
    public class ExcelFile
    {
        public string File { get; set; }
        public string Sheet { get { return IO.Path.GetFileName(File); } }
        public Exception LastError { get; set; }

        public delegate void FillAction(DataTable table, string file, string sheet);

        public bool FillFromTable(DataTable table, FillAction fillAction)
        {
            bool success = false;       // pesimistic variant
            try
            {
                LastError = null;
                if (IO.File.Exists(File))
                {
                    IO.File.SetAttributes(File, IO.FileAttributes.Normal);
                    IO.File.Delete(File);
                }

                fillAction(table, File, Sheet);
                // NPoiExcel.SaveTableToExcelFile(table, File, Sheet);

                success = IO.File.Exists(File);
            }
            catch (Exception ex) { LastError = ex; }

            return success;
        }

        #if NPOI
            public bool FillFromTable(DataTable table)
            {
                bool ret = FillFromTable(table, new FillAction((table, file, sheet) => 
                {
                    NPoiExcel.SaveTableToExcelFile(table, File, Sheet);
                }));
                return ret;
            }
        #endif
    }

#if NPOI
    using NPOI.HSSF.UserModel;

    public static class NPoiExcel
    {

        static string GetString(object o)
        {
            if (o == null)
                return String.Empty;
            return o.ToString();
        }

        public static void SaveTableToExcelFile(DataTable dt, string filename, string sheetName)
        {
            CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            // NPOI.HSSF.UserModel;
            HSSFWorkbook wb = new HSSFWorkbook();

            var ws = wb.CreateSheet(sheetName);

            // column headers  
            var row = ws.CreateRow(0);
            for (int colNdx = 0; colNdx < dt.Columns.Count; colNdx++)
            {
                row.CreateCell(colNdx).SetCellValue(dt.Columns[colNdx].ColumnName);
            }

            // export data  
            for (int rowNdx = 0; rowNdx < dt.Rows.Count; rowNdx++)
            {
                var rowTable = ws.CreateRow(rowNdx + 1);
                for (int colNdx = 0; colNdx < dt.Columns.Count; colNdx++)
                {
                    string value = dt.Rows[rowNdx][colNdx].ToString();
                    Type type = dt.Rows[rowNdx][colNdx].GetType();

                    if (type.Equals(typeof(System.String)))
                    {
                        rowTable.CreateCell(colNdx).SetCellValue(value);
                    }
                    else
                    {
                        double number;
                        if (double.TryParse(value, out number))
                        {
                            var cell = rowTable.CreateCell(colNdx, NPOI.SS.UserModel.CellType.NUMERIC);
                            cell.SetCellValue(number);
                        }
                        else if (value.Length > 0 && value[0] == '\'' && double.TryParse(value.Substring(1), out number))
                            rowTable.CreateCell(colNdx).SetCellValue(value.Substring(1));
                        else
                            rowTable.CreateCell(colNdx).SetCellValue(GetString(dt.Rows[rowNdx][colNdx]));
                    }
                }
            }

            /* 
             * if (fileNames == null)
                fileNames = new List<string>();
            fileNames.Add(filename); */

            // write Excel BIFF binary stream
            IO.FileStream fs = new IO.FileStream(filename, IO.FileMode.Create);
            wb.Write(fs);
            fs.Close();

            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

    }
#endif

}