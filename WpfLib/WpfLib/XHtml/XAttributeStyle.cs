using System;
using System.Collections.Generic;
using System.Linq;

using System.Xml.Linq;

namespace Ai.XHtml
{
    public class XAttributeStyle
    {
        public static XAttributeStyle Style(XElement el)
        {
            return !el.HasAttributes ? null
                : new XAttributeStyle(el.Attributes("style"));
        }

        public XAttributeStyle(IEnumerable<XAttribute> attr)
        {
            if (attr.Any())
                this.attr = attr.First();
        }

        public XAttributeStyle(XAttribute attr)
        {
            this.attr = attr;
        }

        private XAttribute attr;

        public bool Contains(string style)
        {
            if (attr == null)
                return false;
            var styleStr = attr.ToString().ToUpper().Replace(": ", ":");
            return styleStr.Contains(style.ToUpper());
        }

    }

    /*
     
     * 
    IEnumerable<XNode> GetDescendants(XContainer node, // XName name, 
                    bool self = false)
    {
        if (self)
        {
            var e = node as XNode;
            if (e != null) // if (name == null || e.name == name) 
                yield return e;
        }

        XNode n = node;
        XContainer c = node;
        while (true)
        {
            if (c != null && c.Content is XNode)
            {
                n = ((XNode)c.Content).Next;
            }
            else
            {
                while (n != node && n == n.Parent.Content)
                {
                    n = n.Parent;
                    if (n.Parent == null || n == node)
                        yield break;
                }
                if (n == node || n.Parent == null) 
                    yield break;
                n = n.Next;
            }

            var e = n as XNode;
            if (e != null) // && (name == null || e.name == name)) 
                yield return e;
            c = n as XContainer;
        }
    }

    internal IEnumerable<XNode> GetDescendantNodes(bool self)
    {
        if (self) yield return this;
        XNode n = this;
        while (true)
        {
            XContainer c = n as XContainer;
            XNode first;
            if (c != null && (first = c.FirstNode) != null)
            {
                n = first;
            }
            else
            {
                while (n != null && n != this && n == n.parent.content) n = n.parent;
                if (n == null || n == this) break;
                n = n.next;
            }
            yield return n;
        }
    */

}
