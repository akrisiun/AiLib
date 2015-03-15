using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Ai.Entity
{
    public static class SqlXLinq
    {
        public static SqlCommand StoredProc(string sql, SqlConnection conn,
                    Action<SqlCommand> passParam = null)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = sql;

            if (passParam != null)
                passParam(cmd);

            return cmd;
        }

        public static XDocument ExecuteXml(this SqlCommand cmd, string RootName = "Root")
        {
            using (XmlReader reader = cmd.ExecuteXmlReader())
            {
                return SqlXLinq.MultipleNodes(reader, RootName);
            }

        }

        public static XDocument MultipleNodes(XmlReader reader, string RootName = "Root")
        {
            var doc = new XDocument(new XElement(RootName));

            var xn = new XPathDocument(reader).CreateNavigator();
            XPathNodeIterator iterator = xn.Select("/*");
            foreach (XPathNavigator item in iterator)
            {
                doc.Root.Add(XElement.Load(item.ReadSubtree()));
            }

            return doc;
        }

        public static XDocument Transform(XDocument doc, string fileXslt)
        {
            XDocument xslt = XDocument.Load(fileXslt);  // check Xslt file is valid

            XslCompiledTransform trans = new XslCompiledTransform();

            // XsltArgumentList xsltArg = new XsltArgumentList();
            // xsltArg.AddExtensionObject("urn:script-items", obj);

            trans.Load(fileXslt); //  xslt.CreateReader());

            XDocument outputDoc = new XDocument();
            using (var writer = outputDoc.CreateWriter())
            {
                trans.Transform(doc.CreateReader(), writer);
            }

            return outputDoc;
        }

    }
}
