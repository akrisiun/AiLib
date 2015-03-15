using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Ai.Entity
{
    public class DbMapperXElem : DbDataMapHelper<XElement>
    {
        public DbMapperXElem(DbDataReader dataReader)
            : base()
        {
            fields = SqlFieldArray.GetFields(dataReader);
        }

        public XElement Get(IDataRecord record, string elementName)
        {
            object[] objVal = DbRecordArray(record.FieldCount);
            record.GetValues(objVal);

            var val = new XElement(elementName);

            foreach (KeyValuePair<string, SqlFieldInfo> pair in fields)
            {
                object fieldValue = objVal[pair.Value.Ordinal].Equals(DBNull.Value) ? null :
                              objVal[pair.Value.Ordinal];
                val.Add(new XElement(pair.Key, fieldValue));
            }

            return val;
        }

    }
}
