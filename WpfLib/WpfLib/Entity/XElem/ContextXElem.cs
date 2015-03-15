using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Ai.Entity
{
    public static class ContextXElem
    {
        public static XElement Convert(this ExpandoObject obj, string rootName)
        {
            XElement root = new XElement(rootName);
            var numerator = obj.Keys().GetEnumerator();
            while (numerator.MoveNext())
            {
                string key = numerator.Current;
                object value = (obj as IDictionary<string, object>)[key];
                root.Add(new XElement(key, value));
            }
            return root;
        }


        public static XElement ExecMergeXElem(this Context db, object namedParam
                , string[] elemNames
                , Action<SqlCommand> setupCmd = null)
        {
            KeyValuePair<SqlDataReader, DbEnumeratorData<ExpandoObject>> firstSet = db.ExecDyn(namedParam, setupCmd);
            var reader = firstSet.Key;
            var numerator = firstSet.Value;

            Guard.Check(elemNames.Length >= 2);
            var retElement = new XElement(elemNames[0]);
            var rootName = elemNames[1];

            var depth = reader.Depth;
            int index = 1;

            while (numerator.MoveNext())
            {
                var expando = numerator.Current as ExpandoObject;
                retElement.Add(expando.Convert(rootName));
            }
            numerator.Dispose();

            while (!reader.IsClosed)
            {
                if (!reader.NextResult())
                    break;

                index++;
                rootName = elemNames.Length <= index ? "Node" + index.ToString()
                         : elemNames[index];
                var result2 = DbEnumeratorData.GetResultDyn(() => { return reader; });
                
                while (result2.MoveNext())
                { 
                    // yield return 
                    XElement elem = result2.Current.Convert(rootName);
                    retElement.Add(elem);
                }

                result2.Dispose();
            }

            return retElement;
        }
    }
}
