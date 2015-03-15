using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Ai.Entity
{
    public static class ContextMultiXml
    {
        public static MultiResult<XElement> MultiXElem(this Context db, object sqlProcNamed)
        {
            var proc = SqlProcExt.ProcNamed(sqlProcNamed);
            proc.Context = db;
            return MultiXElem(proc);
        }

        // Sql data set to multiple XML
        public static MultiResult<XElement> MultiXElem(this SqlProc proc)
        {
            var result = new MultiResult<XElement>();
            return result.Prepare(proc);
        }

    }
}
