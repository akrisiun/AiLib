using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ai;
using Ai.Entity;
using Ai.Reflection;
using System.Windows.Controls;

namespace WpfExec.Entity.Bind
{
    public static class HtmlResult
    {
        public static void BodySet<T>(this WebBrowser browser, T body) where T : class 
        {
            var html = new XElement("html", Head, body);
            browser.NavigateToString(html.ToString());
        }

        public static XElement Head
        {
            get
            {
                return new XElement("head",
                    XElement.Parse(@"<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8""/>")
                    );
            }
        }

        public static T Convert<T>(IEnumerable<ExpandoObject> list) where T : class
        {
            XElement table = new XElement("table", new XAttribute("style", "border: 1px solid gray;"));
            XElement head = new XElement("tr");

            var numerator = list.GetEnumerator();
            if (numerator == null)
                return default(T);
            if (!numerator.MoveNext())
                return default(T);
            ExpandoObject first = numerator.Current;
            if (first == null)
                return default(T);

            // Ai.Reflection;
            foreach (string item in ExpandoUtils.Keys(first))
            {
                head.Add(new XElement("th",
                    new XElement("nobr", HtmlAgilityPack.HtmlDocument.HtmlEncode(item))
                    ));
            }
            table.Add(head);

            do
            {
                var item = numerator.Current as ExpandoObject;
                var row = new XElement("tr");
                foreach (string itemKey in item.Keys())
                {
                    var td = new XElement("td");
                    string itemValueOrig = item.DynValue<string>(itemKey);
                    if (!string.IsNullOrWhiteSpace(itemValueOrig))
                    {
                        var itemValue = itemValueOrig.TrimEnd();
                        if (itemValue.Contains(' ') && itemValue.Length < 30)
                            // HtmlAgilityPack.HtmlDocument.HtmlEncode
                            td.Add(new XElement("nobr", (itemValue)));
                        else if (itemValue.Length > 170)
                            td.Add(new XElement("div"
                                , new XAttribute("style", "min-width: 400px;")
                                , itemValue));
                        else if (itemValue.Length > 70)
                            td.Add(new XElement("div"
                                , new XAttribute("style", "min-width: 200px;")
                                , itemValue));
                        else
                            td.Value = (itemValue);
                    }
                    row.Add(td);
                }
                table.Add(row);
            }
            while (numerator.MoveNext());

            // body.Add(table); return body as T;
            return table as T;
        }
         
    }
}
