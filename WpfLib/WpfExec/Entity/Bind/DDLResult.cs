using Ai.Entity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WpfExec.Entity.Bind
{
    public static class DDLResult
    {
        public static T CS<T>(SqlDataReader reader, SqlField[] fields, string name = "Noname") where  T : class 
        {
            var div = new XElement("div");
            div.Add("public class " + name + "{");
            div.Add(new XElement("br"));

            foreach (SqlField field in fields)
            {
                string typ = field.Nullable ? "?" : "" + field.Type.Name;
                if (typ.Equals("String") || typ.Equals("Byte") || typ.Equals("Decimal") || typ.Equals("Int"))
                    typ = typ.ToLower();

                var line = "    public " + typ + " " + field.Name + " { get; set; }";
                if (field.MaxLength > 0)
                    line += " // " + field.MaxLength.ToString();
                div.Add(line);
                div.Add(new XElement("br"));
            }

            div.Add("}");
            div.Add(new XElement("br"));
            return div as T;
        }
    }
}
