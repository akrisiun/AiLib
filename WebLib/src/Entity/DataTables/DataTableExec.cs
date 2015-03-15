using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    public static class DataTableExec
    {
        public static DataTable Execute(this Context context, SqlProc procedure, bool parseError)
        {
            if (parseError)
            {
                try
                {
                    context.LastError = null;
                    if (procedure.Connection == null)
                    {
                        procedure.Connection = context.SqlConnection;
                        if (procedure.Connection == null)
                            return null;
                    }

                    Trace.WriteLine("SqlProc: " + procedure.CmdText);

                    return procedure.Result();
                }
                catch (Exception ex)
                {
                    Trace.Write("Error in " + procedure.CmdText + " : " + ex.Message);
                    context.LastError = ex;
                }
            }
            return null;
        }

        public static DataTable Execute(this Context context, SqlProc procedure)
        {
            if (procedure.Connection == null)
            {
                procedure.Connection = context.SqlConnection;
                if (procedure.Connection == null 
                    || string.IsNullOrEmpty(procedure.Connection.ConnectionString))
                    return null;
            }

            Trace.WriteLine("SqlProc: " + procedure.CmdText);

            return procedure.Result();
        }

    }
}
