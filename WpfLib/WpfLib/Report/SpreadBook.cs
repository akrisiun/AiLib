using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if XLSX

using DocumentFormat;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.IO;

namespace Ai.Report
{
    // <package id="ClosedXML" version="0.75.0" targetFramework="net45" />
    // <package id="DocumentFormat.OpenXml" version="2.5" targetFramework="net45" />

    // using NPOI.HSSF.UserModel;
    // using NPOI.SS.UserModel;
    // namespace Ai.Report.NPoi

    // XLSX files: Install-Package DocumentFormat.OpenXml 
    public class SpreadBook : IWorkbookBase, IDisposable
    {
        // Office2010. public class Table : DocumentFormat.OpenXml.OpenXmlLeafElement
        DocumentFormat.OpenXml.Packaging.SpreadsheetDocument doc;
        // NPOI.HSSF.UserModel.HSSFWorkbook workbook;

        public SpreadBook() { }
        public SpreadBook(SpreadsheetDocument workbook)
        {
            this.doc = workbook;
            CurrentSheet = 0;

            // IEnumerable<WorksheetPart> WorksheetParts { get; }
        }

        public SpreadsheetDocument Book { get { return doc; } }

        public void Write(Stream exportData)
        {
            Guard.Check(doc != null, "XlsX Workbook Write error");
            // workbook.Write(exportData);
        }

        public string Name { get; set; }
        public string FileName { get; private set; }
        public int CurrentSheet { get; private set; }
        public int CurrentRow { get; private set; }
        public int CurrentColumn { get; private set; }

        #region TODO

        public int WorkSheetCount()
        {
            return 0;
        }

        public virtual IList<ISheetBase> WorkSheets
        {
            get
            {
                return XlsxSheet.SheetList(this);
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public virtual ISheetBase ActiveSheet
        {
            get
            {
                return new XlsxSheet(this, CurrentSheet);
            }
            set
            {
                var sheet = value as XlsxSheet;
                Guard.Check(sheet.Owner == this);
                // int index = sheet.IndexOf(sheet.Selected);
                // CurrentSheet = index;
            }
        }

        public void SetSheet(int index)
        {
            Guard.Check(index >= 0 && index <= WorkSheetCount()); // SpreadBook.Count);
            CurrentSheet = index;
        }

        public virtual IRowBase ActiveRow
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public virtual ICellBase ActiveCell
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        #endregion

        public virtual void Open(string fileName)
        {
            Dispose();
            using (var stream = File.OpenRead(fileName))
            {
                // {"The supplied data appears to be in the Office 2007+ XML. You are calling the part 
                // of POI that deals with OLE2 Office Documents. You need to call a different part of POI 
                // to process this data (eg XSSF instead of HSSF)"}

                FileName = fileName;

                // System.IO.Packaging.Package
                doc = SpreadsheetDocument.Open(stream, false);
            }

            CurrentSheet = 0;
            CurrentRow = 0;
            CurrentColumn = 0;
            Name = Path.GetFileNameWithoutExtension(FileName);
        }

        public void Dispose()
        {
            if (doc != null)
                doc.Dispose();
            doc = null;    // dispose
        }


        public ISheetBase CreateSheet(string sheetName)
        {
            throw new NotImplementedException();
        }


        public void ReadStream(Stream importData, string fileName)
        {
            throw new NotImplementedException();
        }
    }

    public class XlsxSheet : ISheetBase // , IList<ISheet> 
    {
        private SpreadBook owner;
        private int selected;

        public XlsxSheet(SpreadBook owner, int index = 0)
        {
            selected = index;
            this.owner = owner;
        }

        /* public XslxSheet(SpreadBook owner, ISheet sheet)
        {
            selected = sheet.Workbook.IndexOf(sheet);
            this.owner = owner;
        }
        */

        public static IList<ISheetBase> SheetList(SpreadBook owner)
        {
            var list = new List<XlsxSheet>();
            int index = -1;
            while (index < owner.WorkSheets.Count)
            {
                list.Add(new XlsxSheet(owner, (index++))
                        );
            }
            return list as IList<ISheetBase>;
        }

        public IEnumerator<ISheetBase> Enumerator()
        {
            yield return new XlsxSheet(owner, 0);

            // todo
            // for (int i = 0; i < owner.SpreadBook.Count; i++)
        }

        public string SheetName
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }


        public IWorkbookBase Owner { get { return owner; } }

        #region ISheet

        // public ISheet Selected { get { return owner.SpreadBook[selected]; } }
        /*
        public IEnumerator<ISheet> GetEnumerator()
        {
            var num = (this as System.Collections.IEnumerable).GetEnumerator();
            yield break;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return owner.SpreadBook.GetEnumerator();
        }

        public int IndexOf(ISheet item)
        {
            return owner.SpreadBook.IndexOf(item);
        }

        public void Insert(int index, ISheet item)
        {
            owner.SpreadBook.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            owner.SpreadBook.RemoveAt(index);
        }

        public ISheet this[int index]
        {
            get
            {
                return owner.SpreadBook[index];
            }
            set
            {
                owner.SpreadBook[index] = value;
            }
        }

        public void Add(ISheet item) { owner.SpreadBook.Add(item); }
        public void Clear() {  owner.SpreadBook.Clear(); }
        public bool Contains(ISheet item) { return owner.SpreadBook.Contains(item); }

        public void CopyTo(ISheet[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public int Count { get { return owner.SpreadBook.Count; }  }
        public bool IsReadOnly { get { return owner.SpreadBook.IsReadOnly; } }
        public bool Remove(ISheet item) { return owner.SpreadBook.Remove(item); }
         */
        #endregion



        public IRowBase this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        public int RowCount
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        IWorkbookBase ISheetBase.Owner
        {
            get { throw new NotImplementedException(); }
        }

        IRowBase ISheetBase.this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        int ISheetBase.RowCount
        {
            get
            {
                throw new NotImplementedException();
            }
             
        }
    }

}

#endif
