using Ai.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace Ai.Report
{
    public static class ExcelReport
    {
        public static bool ToXls(SqlObjTable data, string fileName, string sheetName = "Report")
        {
            return NPoi.Excel.ToXls(data, fileName, sheetName);
        }
    }

}

namespace Ai.Report.NPoi
{
    using NPOI;
    using HSSF = NPOI.HSSF;

    public static class Excel
    {
        public static bool ToXls(SqlObjTable data, string fileName, string sheetName = "Report")
        {
            var wb = XlsWorkbook(data, fileName, sheetName);
            if (wb == null)
                return false;

            CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            bool success = false;

            // write Excel BIFF binary stream
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);

                FileStream fs = new FileStream(fileName, FileMode.Create);
                wb.Write(fs);
                fs.Close();
                success = true;
            }
            catch (Exception ex)
            {
                Web.Log.WriteException(string.Format("Error saving {0} : {1}", fileName, ex.Message), ex);
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
            return success;
        }

        public static NPOI.HSSF.UserModel.HSSFWorkbook XlsWorkbook(SqlObjTable data, string fileName, string sheetName = "Report")
        {
            CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var wb = new NPOI.HSSF.UserModel.HSSFWorkbook();

            var ws = wb.CreateSheet(sheetName);

            // header columns 
            var rowHead = ws.CreateRow(0);
            foreach (var field in data.Fields)
            {
                rowHead.CreateCell(field.Ordinal).SetCellValue(field.Caption);
            }

            // export data  
            int index = 0;
            foreach (var row in data.Rows)
            {
                int rowNdx = index++; // data.Rows.IndexOf(row);
                var rowItem = ws.CreateRow(rowNdx + 1);

                foreach(var field in data.Fields)
                {
                    int colNdx = field.Ordinal;

                    Type type = field.Type;
                    string value = row[colNdx] as string;

                    if (type.Equals(typeof(string))
                        && field.Name.StartsWith("html")
                        && !string.IsNullOrWhiteSpace(value))
                    {
                        var valueBr = value.Replace("<br/>", "\n");
                        var el = System.Xml.Linq.XElement.Parse(value);

                        var valueRtf = new StringWriter();
                        foreach (XElement item in el.Descendants("div"))
                            valueRtf.Write(item.Value.Replace("\n", " \r\n"));

                        rowItem.CreateCell(colNdx).SetCellValue(valueRtf.ToString());
                    }
                    else 
                    if (type.Equals(typeof(System.String)))
                    {
                        rowItem.CreateCell(colNdx).SetCellValue(value);
                    }
                    else
                    {
                        double number;
                        object valueNum = valueNum = row[colNdx];
                        if (valueNum == null && value == null)
                        {
                            rowItem.CreateCell(colNdx).SetCellValue(String.Empty);
                        }
                        if (valueNum is int)
                        {
                            var cell = rowItem.CreateCell(colNdx, NPOI.SS.UserModel.CellType.Numeric);
                            cell.SetCellValue((valueNum as int?).Value);
                        }
                        else if (valueNum is double)
                        {
                            var cell = rowItem.CreateCell(colNdx, NPOI.SS.UserModel.CellType.Numeric);
                            cell.SetCellValue((valueNum as double?).Value);

                        } else if (double.TryParse(value, out number))
                        {
                            var cell = rowItem.CreateCell(colNdx, NPOI.SS.UserModel.CellType.Numeric);
                            cell.SetCellValue(number);
                        }
                        else if (value.Length > 0 && value[0] == '\'' && double.TryParse(value.Substring(1), out number))
                            rowItem.CreateCell(colNdx).SetCellValue(value.Substring(1));
                        else
                        {
                            // var obj = dt.Rows[rowNdx][colNdx];
                            var obj = row[colNdx];
                            rowItem.CreateCell(colNdx).SetCellValue(GetString(obj));
                        }
                    }
                }
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
            return wb;
        }

        static string GetString(object o)
        {
            return (o == null) ? string.Empty : o.ToString();
        }

    }
}
