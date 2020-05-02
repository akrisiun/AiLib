using Ai.Report;
using Ai.Report.Excel;
#if NPOI
using Ai.Report.NPoi;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Ai.Report
{
    public class ExcelTableImport
    {
        public enum DocFormat
        { 
            Xls = 1,
            Xlsx = 2
        }

        public IWorkbookBase Book { get; private set; }
        public DocFormat Format { get; private set; }

#if NPOI
        public virtual void LoadNPoi(string file)
        {
            Format = DocFormat.Xls;
             if (!File.Exists(file))
                 throw new ArgumentException("File " + file + " error");

            Book = new NPoiWorkbook();
            Book.Open(file);
        }

        public virtual IWorkbookBase LoadNPoiStream(Stream fileXls, string fileName)
        {
            Format = DocFormat.Xls;
            Guard.Check(fileXls != null);

            Book = new NPoiWorkbook();
            var npoiBook = Book as NPoiWorkbook;
            npoiBook.ReadStream(fileXls, fileName);
            return Book;
        }

#endif
#if XLSX
        public virtual void LoadXlsx(string file)
        {
            Format = DocFormat.Xlsx;
            if (!File.Exists(file))
                throw new ArgumentException("File " + file + " error");

            Book = new SpreadBook();
            Book.Open(file);
        }
#endif

    }
}