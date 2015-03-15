using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Ai.Entity;
using Ai.Reflection;

namespace WpfExec.Entity.Bind
{
    public static class ServerList
    {
        public static ObservableCollection<ServerDBName> Servers()
        {
            var connDefault = ConfigurationManager.ConnectionStrings["Default"];
            var servers = new ObservableCollection<ServerDBName> { 
                new ServerDBName { Server = connDefault.Name, 
                    DbName = ""
                }
            };
            return servers;
        }

        public static void ComboSelect(ServerDBName selected, object dataContext)
        {
            Context.Instance = null;   // Dispose
            Context db = new Context(fileOrServerOrConnection: String.Format("name={0}", selected.Server));
                        //(select.Open(selected.ServerEnum, selected.DbName, openConnection: false);
            (dataContext as ExpandoObject).DynValueSet("db", db);
            dataContext.SetValue<string>("dbName", db.DbName);
        }

    }

    public class ServerDBName
    {
        public string Server { get; set; }
        public string DbName { get; set; }

        public string Caption { get { return DbName + " " + Server; } }
        public override string ToString() { return Caption; }
    }

}
