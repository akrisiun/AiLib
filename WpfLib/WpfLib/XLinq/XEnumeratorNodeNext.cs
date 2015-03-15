using Ai.XHtml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Ai.XLinq
{
    // Enumerator with NextNode
    public class XEnumeratorNodeNext : IEnumerator<XNode>, IEnumerable<XNode>
    {
        public XEnumeratorNodeNext(XContainer source)
        {
            nodes = source.DescendantNodes();
            Reset();
        }

        protected IEnumerable<XNode> nodes;
        protected IEnumerator<XNode> iterate;

        public XNode Current
        {
            get { return iterate == null ? null : iterate.Current; }
        }

        #region IEnumerator, IEnumerable interfaces

        object IEnumerator.Current { get { return Current; } }
        public virtual bool MoveNext() { return iterate.MoveNext();  }

        public virtual void Reset(XContainer root = null)
        {
            if (root == null)
                Reset();
            else
                iterate = root.DescendantNodes().GetEnumerator();
        }
        public void Reset()
        {
            iterate = nodes.GetEnumerator();
        }
        public virtual void Dispose()
        {
            if (iterate != null)
                iterate.Dispose();
            iterate = null;
        }
        public IEnumerator<XNode> GetEnumerator() { return iterate; }
        IEnumerator IEnumerable.GetEnumerator() { return iterate; }

        #endregion

        #region Node Extensions
        public XNode NextNode
        {
            get
            {
                XNode current = iterate.Current;
                if (current == null)
                    return null;

                if (current is XContainer)
                {
                    var cnt = current as XContainer;
                    if (cnt.FirstNode != null)
                        return cnt.FirstNode;
                }

                return current.NextNode;
            }
        }

        public IEnumerator<XElement> ElementsTree()
        {
            iterate = nodes.GetEnumerator();
            while (iterate.MoveNext())
            {
                if (iterate.Current is XElement)
                    yield return iterate.Current as XElement;
            }
        }

        public static IEnumerator<XNode> GetDescendants(XContainer container, bool self = false)
        {
            // https://github.com/dotnet/corefx/blob/master/src/System.Xml.XDocument/src/System/Xml/Linq/XNode.cs
            // var c = new System.Xml.MinLinq.XContainer().DescendantNodes();
            return container.DescendantNodes().GetEnumerator();
        }

        public IList<XNode> NodeList
        {
            get
            {
                var clone = nodes;
                return System.Linq.Enumerable.ToList<XNode>(clone);
            }
        }

        #endregion
    }

}
