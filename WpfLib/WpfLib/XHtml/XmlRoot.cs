using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Ai.XHtml
{
    public static class XmlRoot
    {
        public static XmlDocument RootXml(string encoding)
        {
            var xml = new XmlDocument();

            var elRoot = xml.CreateElement("Root");
            xml.AppendChild(elRoot);

            XmlDeclaration xmlDeclaration = xml.CreateXmlDeclaration("1.0", encoding, null);
            xml.InsertBefore(xmlDeclaration, elRoot);

            return xml;
        }

    }
}
