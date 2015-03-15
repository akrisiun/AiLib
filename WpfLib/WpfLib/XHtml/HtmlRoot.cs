using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Ai.XHtml
{
    public class HtmlRoot : IDisposable
    {
        public static XDocument XCharsetHtml(string charset = "utf-8", object bodyContent = null)
        {
            var doc = XDocument.Parse(@"<!DOCTYPE html><html/>");
            // \"-//W3C//DTD XHTML 1.0 Transitional//EN\" 

            var head = new XElement("head"
                , new XElement("meta"
                        , new XAttribute("http-equiv", "Content-Type")
                        , new XAttribute("content", @"text/html; charset=" + charset)
                        )
                );

            doc.Root.Add(head);
            doc.Root.Add(new XElement("body", bodyContent));
            return doc;
        } 

        void IDisposable.Dispose()
        {
            // XmlEl.xmlDoc = null;
        }

        public static string ExtractTag(string htmlText, string tag)
        {
            return StringExt.StrExtract(htmlText, "<" + tag, "</" + tag + ">", ">");
        }

        public static string ExtractBody(string htmlText)
        { 
            return StringExt.StrExtract(htmlText, "<body", "</body>", ">");
        }

        public static string ExtractStyle(string htmlText)
        {
            return StringExt.StrExtract(htmlText, "<style", "</style>", ">");
        }

    }
}
