using Ai.XHtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Ai.XLinq
{
    public class XHtmlCompact
    {
        protected XEnumeratorNodeNext loader;
        public XHtmlCompact(XContainer source)
        {
            loader = new XEnumeratorNodeNext(source);
        }

        public static XElement Parse(XElement obj)
        {
            if (obj == null)
                return null;

            var worker = new XHtmlCompact(obj);
            XEnumeratorNodeNext loader = worker.loader;

            XElement result = new XElement("div");
            if (obj.Attributes().Any())
                result.Add(obj.Attributes().ToList());

            StringBuilder build = new StringBuilder();
            int boldLevel = 0;

            XElement contentBefore = null;
            XElement breakAfter = null;
            if (loader.Current != null)
                loader.Reset();

#if DEBUG
            //  int count = System.Linq.Enumerable.Count<XNode>(loader);
            int indexNode = -1;
#endif
            XElement contentEl = null;

            while (loader.MoveNext())
            {
#if DEBUG
                indexNode++;
#endif
                XNode elObj = loader.Current;
                XElement el = elObj as XElement;

                if (el == null)
                {
                    if (elObj is XText && elObj.Parent != contentBefore
                        && (elObj as XText).Value != null)
                    {
                        var nodeText = elObj as XText;
                        string text = nodeText.Value.Replace("&nbsp;", ((char)160).ToString());
                        build.Append(text);
                    }

                    if (elObj.Parent == breakAfter)
                    {
                        build.Append(Environment.NewLine);
                        breakAfter = null;
                    }

                    if (elObj.Parent is XElement &&
                        !elObj.Parent.HasElements && contentEl != elObj.Parent)
                    {
                        XElement item = new XElement(elObj.Parent.Name, elObj);
                        contentEl = elObj.Parent;
                        if (contentEl.Parent == null || contentEl.Parent.Name != "a")
                            result.Add(contentEl);
                    }
                    else
                        contentBefore = elObj as XElement ?? new XElement("span", elObj);

                    continue;
                }
                else if (contentBefore != null && contentEl != contentBefore
                         && !string.IsNullOrWhiteSpace(contentBefore.Value))
                {
                    contentEl = contentBefore;
                    result.Add(contentEl);
                }

                XAttributeStyle styleUpper = new XAttributeStyle(el.Attributes("style"));
                // FONT-WEIGHT: bold; FONT-STYLE: italic -> FONT-WEIGHT:BOLD; FONT-STYLE:ITALIC

                string local = el.Name.LocalName.ToLower();
                bool isBold = local == "b" || local == "strong" || styleUpper.Contains("FONT-WEIGHT:BOLD");
                bool isItalic = local == "i" || local == "italic" || styleUpper.Contains("FONT-STYLE:ITALIC");
                bool isBr = (local == "br" || local == "p" && !string.IsNullOrWhiteSpace(el.Value));

                if (isBold)
                {
                    //if (boldLevel == 0)
                    //    boldPositions.Add(new XEnumeratorNodeNext.PosPair { posFrom = build.Length, posTo = -1 });
                    boldLevel++;
                }

                if (el.Attributes("style").Any())
                {
                    el.RemoveAttributes();
                    if (isBold || isItalic)
                    {
                        el.SetAttributeValue("style",
                            (isBold ? "FONT-WEIGHT:BOLD" + (isItalic ? ";" : "") : "")
                            + (isItalic ? "FONT-STYLE:ITALIC" : "")
                            );
                    }
                }

                if (!el.HasElements && el.Value != null)
                {
                    contentBefore = el;
                    string text = el.Value.Replace("&nbsp;", ((char)160).ToString());

                    if (text.Length > 0)
                    {
                        if (build.Length > 2)
                        {
                            string lastChar = build.ToString().Substring(build.Length - 2, 2);

                            if (contentBefore != null
                                && !lastChar.EndsWith(" ") && !lastChar.EndsWith(Char.ToString((char)160))
                                && !lastChar.EndsWith(Environment.NewLine)
                                && !text.StartsWith(" "))
                                build.Append(" ");  // space between <span> content
                        }

                        build.Append(text);

                        if (el is XElement && contentEl != el)
                        {
                            if (el.Parent != null && el.Parent.Name == "a")
                                contentEl = new XElement("a", el);
                            else
                                contentEl = el;

                            result.Add(contentEl);
                        }
                    }
                    if (isBr && build.Length > 0)
                    {
                        build.Append(Environment.NewLine);
                        result.Add(new XElement("br"));
                    }

                }
                else if (isBr && breakAfter == null)
                    breakAfter = el;

                // if (boldLevel > 0) boldLevel--;
            }

            if (contentBefore != null && 
                (contentEl == null || contentEl.Value != contentBefore.Value
                 && !string.IsNullOrWhiteSpace(contentBefore.Value)))
            {
                contentEl = contentBefore;
                result.Add(contentEl);
            }

            return result;
        }

    }
}
