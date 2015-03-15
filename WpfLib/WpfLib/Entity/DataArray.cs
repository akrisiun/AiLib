using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ai.Entity
{
    public class DataArray : IList<object[]>, IEnumerable<object[]>, IDisposable
    {
        #region ctor
        protected List<object[]> array;

        static DataArray()
        {
            Empty = new DataArray();
        }
        public static DataArray Empty;

        public DataArray()
        {
            array = new List<object[]>();
        }
        
        void IDisposable.Dispose() {
            array = null;
        }
        #endregion

        public static DataArray Read(IEnumerable<object[]> array)
        {
            var data = new DataArray();

            foreach(var item in array)
                data.array.Add(item);
            return data;
        }

        public IList<object[]> Array { get { return array; } }

        #region IList
        public int IndexOf(object[] item)
        {
            return array.IndexOf(item);
        }

        public void Insert(int index, object[] item)
        {
            array.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            array.RemoveAt(index);
        }

        public object[] this[int index]
        {
            get
            {
                return array[index];
            }
            set
            {
                array[index] = value;
            }
        }

        public void Add(object[] item)
        {
            array.Add(item);
        }

        public void Clear()
        {
            array.Clear();
        }

        public bool Contains(object[] item)
        {
            return array.Contains(item);
        }

        public void CopyTo(object[][] arrayResult, int arrayIndex)
        {
            array.CopyTo(arrayResult, arrayIndex);
        }

        public int Count
        {
            get { return array.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(object[] item)
        {
            return array.Remove(item);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return array.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        public virtual XDocument GetXml(string[] names)
        {
            var doc = new XDocument(new XElement(names[(int)IdxNames.Root]));
            foreach (object[] row in Array)
            {
                if (row == null || row.Length == 0)
                    continue;

                XElement rowEl = new XElement(names[(int)IdxNames.Row]);
                for (int i = 0; i < row.Length; i++)
                {
                    var el = new XElement(names[(int)IdxNames.FirstField + i]);

                    if (row[i] is string 
                        && (row[i].ToString().Contains("</") || row[i].ToString().Contains("/>")))
                        el.Add(XElement.Parse(row[i] as string));
                    else 
                        el.SetValue(row[i]);
                    
                    rowEl.Add(el);
                }
                doc.Root.Add(rowEl);
            }
            return doc;
        }

        public enum IdxNames : int { 
            Root = 0,
            Row = 1,
            FirstField = 2
        }
    }

    public class DataArrayFields : DataArray
    {
        public SqlField[] Fields { get; set; }

        public XDocument GetXml(string rootName = "Root")
        {
            string[] names = new string[] { rootName };
            System.Array.Resize<string>(ref names, 1 + Fields.Length);
            for (int i = 0; i < Fields.Length; i++)
                names[1 + i] = Fields[i].Caption;

            return GetXml(names);
        }
    }

}
