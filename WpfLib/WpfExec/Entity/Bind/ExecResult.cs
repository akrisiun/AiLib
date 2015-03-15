using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Data;
using System.Windows.Controls;
using System.Threading;

using Ai.Entity;
using Ai.Reflection;
using Ai.XHtml;

namespace WpfExec.Entity.Bind
{

    public static class ExecResult
    {
        public static async void Exec(ExecWindow window, string cmd, WebBrowser htmlCtrl)
        {
            var data = window.DataContext as ExpandoObject;
            Ai.Reflection.ExpandoConvert.SetValue<string>(data, "command", cmd);

            Context db = data.DynValue("db") as Context;

            SqlCommand execProc = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = cmd,
                Connection = db.SqlConnection 
            };
            execProc.CommandTimeout = 5; // sec
            var token = new CancellationToken();

            var task = Task.Factory.StartNew<object>(() =>
                {
                    string error = string.Empty;
                    DbEnumeratorData<ExpandoObject> resSql = db.ExecDynCmd(execProc, onError: (err) =>
                    {
                        error = err.Message;
                    });

                    var head = new XElement("h4", cmd);
                    
                    string nameProc = StringExt.StrExtract(cmd.Replace("EXEC ", ""), "", " ");
                    XElement bodyRes = new XElement("body", head);
                    XElement bodyCS = new XElement("body", head);
                    
                    List<ExpandoObject> firstSql = FillHtmls(resSql, nameProc, token, bodyRes, bodyCS
                        , onError: (err) => error = err.Message);

                    var resultObj = new { firstSql =  firstSql, 
                            bodyRes = bodyRes, Error = error, bodyCS = bodyCS };
                    return resultObj;
                }, 
                cancellationToken: token, creationOptions: TaskCreationOptions.LongRunning, 
                scheduler: TaskScheduler.Current );

            await task;
            var res = task.Result;
            var resError = res.GetValue<string>("ERROR");
            if (resError.Length > 0)
            {
                var body = new XElement("body");
                body.Add(cmd); body.Add(new XElement("br"));
                body.Add(resError ?? "Unknown error");
                htmlCtrl.NavigateToString(new XElement("html", body).ToString());
                return;
            }

            var resBodyCS = res.GetValue<XElement>("bodyCS");
            window.result.webCS.BodySet<XElement>(resBodyCS);

            var resBodyRes = res.GetValue<XElement>("bodyRes");
            HtmlResult.BodySet<XElement>(htmlCtrl, resBodyRes);

            var firstResult = res.GetValue<List<ExpandoObject>>("firstSql");
            ExpandoObject firstObj = firstResult.ElementAt(0);

            var grid = window.result.grd1;
            Ai.Wpf.GridDataSource.ToDataSource(grid, firstResult, firstObj);
            //grid.SelectionMode = DataGridSelectionMode.Extended;
            //grid.SelectionUnit = DataGridSelectionUnit.CellOrRowHeader;
            // SelectionMode="Extended" or SelectionMode="Multiple" 
        }

        public static List<ExpandoObject> FillHtmls(DbEnumeratorData<ExpandoObject> resSql,
                    string nameProc, CancellationToken token,
                    XElement bodyRes, XElement bodyCS, Action<Exception> onError)
        {

            List<ExpandoObject> resultObj = new List<ExpandoObject>(System.Linq.Enumerable.Empty<ExpandoObject>());
            if (!resSql.ReaderAvailable)
                return resultObj;

            int result = -1;
            try
            {
                if (resSql.First == null)
                    resSql.MoveNext();
                resultObj = System.Linq.Enumerable.ToList<ExpandoObject>(resSql);
                resSql.Reset();

                do
                {
                    result++;
                    var list = resSql;
                    if (list.First == null)
                        list.MoveNext();
                    var first = list.First;
                    list.Reset();

                    var reader = resSql.Reader;
                    SqlField[] fields = SqlFieldArray.GetArray(reader);
                    // DataTable GetSchemaTable();

                    var cs = DDLResult.CS<XElement>(reader, fields,
                                name: String.Format("{0}{1}", nameProc, result == 0 ? "" : result.ToString()));
                    bodyCS.Add(cs);         // DDL
                    var listHtml = HtmlResult.Convert<XElement>(list as IEnumerable<ExpandoObject>);
                    bodyRes.Add(listHtml);
                    if (fields.Length == 0 || !resSql.NextResult())
                        break;
                }
                while (!token.IsCancellationRequested && result < 10);
            }
            catch (Exception err) { onError(err); }

            return resultObj;
        }

    }
}


//  ItemsSource="{Binding Source=list}"  AutoGenerateColumns="False" CanUserResizeColumns="True">
//  <DataGrid.Columns>                
//        <DataGridTextColumn Header="ID" Binding="{Binding ID}"/>
//        <DataGridTextColumn Header="Date" Binding="{Binding Date}"/>
//   </DataGrid.Columns>
//</DataGrid>

