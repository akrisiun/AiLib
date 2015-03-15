using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ai.XHtml
{
    public class TagsList : IDisposable
    {
        public static TagsList Create()
        {
            return new TagsList();
        }

        public string InnerHtml
        {
            get { return ToString(); }
            set
            {
                var nodes = Nodes ?? new List<object>();
                nodes.Clear();
                nodes.Add(value);
                Nodes = nodes;
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (Nodes == null || Nodes.Count == 0) return true;
                foreach (var node in Nodes)
                    if (!string.IsNullOrWhiteSpace(node.ToString()))
                        return false;

                return true;
            }
        }

        public override string ToString()
        {
            string ret = String.Empty;
            if (Nodes == null || Nodes.Count == 0)
                return ret;

            foreach (var item in Nodes)
                ret += item.ToString() + Environment.NewLine;

            return ret;
        }

        public List<object> Nodes { get; set; }

        public void Add(object obj)
        {
            var nodes = Nodes ?? new List<object>();
            nodes.Add(obj);
            Nodes = nodes;
        }

        public static TagsList operator +(TagsList list, object obj)
        {
            list.Add(obj);
            return list;
        }

        public void Dispose()
        {
            Nodes = null;
        }
    }
}
