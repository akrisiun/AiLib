using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Ai.Entity
{

    public class SqlField
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public Type Type { get; set; }
        public int Ordinal { get; set; }

        public int? Width { get; set; }
        public int? MaxLength { get; set; }

        public string CssClass { get; set; }
        public bool Hide { get; set; }

        public override string ToString()
        {
            return Ordinal + ":" + Name + " " + Type.FullName;
        }

        public HtmlString RenderTH()
        {
            var xml = new XElement("th", 
                    Caption != null && Caption.Contains("<") ? XElement.Parse(Caption) as object
                    : Caption ?? Name);
            if (CssClass != null)
                xml.SetAttributeValue("class", CssClass);

            return new HtmlString(xml.ToString());
        }

        public HtmlString RenderTD(object[] rowData)
        {
            if (rowData[Ordinal] == null || rowData[Ordinal] == DBNull.Value)
                return new HtmlString(new XElement("td", String.Empty).ToString());

            if (this.Name.StartsWith("html") && (rowData[Ordinal] as string).Length > 10)
            {
                var xml = new XElement("td", XElement.Parse(rowData[Ordinal] as string));
                return new HtmlString(xml.ToString().Replace("&lt;br/&gt;", "<br/>"));
            }

            var str = new HtmlString(new XElement("td", rowData[this.Ordinal]).ToString());
            return str;
        }
    }

}
