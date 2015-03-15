using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Ai.XLinq
{
    public class XEnumeratorElement : XEnumeratorNodeNext
    {
        public static XElement CloneRoot(XElement root)
        {
            return new XElement(root.Name, root.Attributes());
        }

        public static XElement Parse(XElement source, Func<XElement, bool> parse)
        {
            var clone = CloneRoot(source);
            var parser = new XEnumeratorElement(source);
            return parser.ChildrenInto(clone, parse);
        }

        public XEnumeratorElement(XContainer source)
            : base(source)
        {
            isMoved = false;
            elBefore = null;
        }

        public XElement Root
        {
            get
            {
                return (base.nodes as XElement) ??
                       (base.nodes.Any() ? base.nodes.First<XNode>().Parent as XElement : null);
            }
        }

        public XElement ChildrenInto(XElement into, Func<XElement, bool> parse)
        {
            Reset();
            elBefore = null;
            var container = Current as XContainer;
            foreach (var child in DescendantsParse(container, parse))
            {
                Guard.Check(!child.Equals(elBefore));
                into.Add(child);
                elBefore = child;
            }
            elBefore = null;
            return into;
        }

        #region Move Next
        public bool MoveAfter(XNode node)    // XNode abstract to XElement or XText
        {
            Guard.Check(node != null);
            if (MoveAfter(iterate, node))
            {
                if (iterate.Current != null) // && iterate.Current == next)
                    isMoved = true;
                return isMoved;
            }
            return false;
        }

        static bool MoveAfter(IEnumerator<XNode> iterate, XNode element)
        {
            var next = element.NextNode;
            if (element.Parent != null && next == element.Parent)
                next = element.Parent.NextNode;
            if (next == null)
            {
                int i = MaxIterations;
                while (iterate.MoveNext() && i > 0) i--;
                return false;
            }

            iterate.MoveNext();
            int iMax = MaxIterations;
            while ((iMax--) > 0 && iterate.Current != null && iterate.Current != next)
                iterate.MoveNext();
            Guard.Check(iMax > 0);
            return (iterate.Current != null && iterate.Current == next);
        }

        XNode elBefore;
        bool isMoved;
        public override bool MoveNext()
        {
            if (isMoved)
            {
                isMoved = false;
                return Current != null;
            }
            return base.MoveNext();
        }
        #endregion

        public static int MaxIterations = 5000;
        IEnumerable<XNode> DescendantsParse(XContainer node, Func<XElement, bool> parse)
        {
            Reset(node);
            XNode root = nodes.Any() ? nodes.First().Parent : null;
            
            int iMax = MaxIterations;
            while (iMax > 0 && MoveNext())
            {
                iMax--;
                XNode itemText = Current as XNode;
                XElement item = itemText as XElement;
                if (item == null)
                {
                    if (itemText != null && itemText.Parent == root)
                        item = new XElement("span", itemText);
                    if (item == null || string.IsNullOrWhiteSpace(item.ToString()))
                        continue;
                }
                bool accept = parse(item);
                if (accept)
                {
                    if (!item.Equals(elBefore))
                        yield return item;
                }
                else
                {
                    var children = item.DescendantNodes().GetEnumerator();
                    while (iMax > 0 && children.MoveNext())
                    {
                        iMax--;
                        XNode itemChText = children.Current;
                        XElement itemCh = itemChText as XElement;
                        if (itemCh == null)
                        {
                            if (itemChText != null && !string.IsNullOrWhiteSpace(itemChText.ToString()))
                                yield return itemChText;
                            continue;
                        }
                        bool acceptCh = parse(itemCh);
                        if (acceptCh)
                        {
                            if (!itemCh.Equals(elBefore))
                                yield return itemCh;
                            else
                            {
                                // todo
                            }
                        }
                        else
                        {
                            using (var numerator = new XEnumeratorElement(itemCh))
                            {
                                foreach (var el in numerator.DescendantsParse(item, parse)) // recursion
                                {
                                    if (!el.Equals(elBefore))
                                        yield return el;
                                    else
                                    {
                                        // todo
                                    }
                                }
                            }
                        }

                        XEnumeratorElement.MoveAfter(children, itemChText);
                    }
                }

                MoveAfter(itemText);
            }
        }

        public IList<XElement> ElementList
        {
            get
            {
                iterate = nodes.GetEnumerator();
                var list = new List<XElement>();
                while (iterate.MoveNext())
                    if (iterate.Current is XElement)
                        list.Add(iterate.Current as XElement);
                return list;
            }
        }

    }

}
