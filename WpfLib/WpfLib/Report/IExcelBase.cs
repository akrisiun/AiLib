using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ai.Report.Excel
{
    public interface IWorkbookBase
    {
        string Name { get; }
        string FileName { get; }

        IList<ISheetBase> WorkSheets { get; }
        ISheetBase ActiveSheet { get; set; }
        IRowBase ActiveRow { get; set; }
        ICellBase ActiveCell { get; }

        ISheetBase CreateSheet(string sheetName);

        void Write(Stream exportData);
        void Open(string fileName);
        void ReadStream(Stream importData, string fileName);
    }

    public interface ISheetBase
    {
        string SheetName { get; set; }
        IWorkbookBase Owner { get; }

        IRowBase this[int index] { get; }
        int RowCount { get; }
    }

    public interface IRowBase
    {
        int RowIndex { get; set; }
        ISheetBase Sheet { get; }
        
        ICellBase this[int index] { get; }
        int ColumnCount { get; }
    }

    public interface ICellBase
    {
        int ColumnIndex { get; }
        IRowBase Row { get; }
    }

}
