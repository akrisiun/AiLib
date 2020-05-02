using Ai.Entity;
using Ai.Reflection;
using Ai.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using mshtml;
using WpfExec.Entity.Bind;
using Ai;
using System.Windows.Input;
using System.Configuration;

namespace WpfExec.Entity
{
    public static class ExecBind
    {
        public static object Data
        {
            get
            {
                var dbName = "SNTXDB";
                var data = new  // Anonymous type
                {
                    sqlserver = "",
                    db = Context.Empty,
                    dbName = dbName,
                    servers = ServerList.Servers(),
                    command = string.Empty
                };
                return data;
            }
        }

        public static void Init(this ExecWindow window)
        {
#if MSHTML
            WebWpfHelper.Prepare(window.result.web1);
            window.result.webCS.Prepare();
            window.result.webTSql.Prepare();
#endif

            window.textCmd.GotFocus += (s, e) =>
            {
                if (!window.isFirst)
                {
                    window.textCmd.Text = "";
                    window.isFirst = true;
                }
            };
            object data = Data;
            var dataMutable = ExpandoConvert.Muttable(data);
            window.DataContext = dataMutable;

            window.cmdExec.Click += (s, e) => window.Exec();
            window.cmdPaste.Click += (s, e) => window.Paste();
            window.textCmd.KeyUp += (s, e) =>
            {
                if (e.Key == Key.Enter 
                    && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
                    && (s as TextBox).Text.Length > 3)
                    window.Exec();
            };
            window.KeyDown += (s, e) =>
                {
                    if (e.Key == Key.F5)
                        window.Exec();
                };
            window.Loaded += (s, e) =>
                {
                    window.result.tab1.SelectedIndex = 1;
                    window.textCmd.Focus();
                };

            // http://stackoverflow.com/questions/622664/what-is-immutability-and-why-should-i-worry-about-it
            var servers = dataMutable.DynValue("servers") as ObservableCollection<ServerDBName>;
            SetupCombo(window.cboServers, servers, window.DataContext);

            var cmd = ConfigurationManager.AppSettings["cmd"];
            if (!string.IsNullOrWhiteSpace(cmd))
            {
                window.isFirst = true;
                window.textCmd.Text = cmd;
            }
        }

        public static void Exec(this ExecWindow window)
        {
            string cmd = window.textCmd.Text;
            WebBrowser htmlCtrl = window.result.web1;

            htmlCtrl.NavigateToString(cmd);
            ExecResult.Exec(window, cmd, htmlCtrl);
        }

        public static void Paste(this ExecWindow window)
        {
            if (!Clipboard.ContainsText())
                return;
            var text = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(text))
                return;
            window.isFirst = true;
            window.textCmd.Text = text;
        }

        static void SetupCombo(ComboBox cboServers, ObservableCollection<ServerDBName> data, object dataContext)
        {
            cboServers.Items.Clear();
            cboServers.DataContext = data;
            if (!data.Any())
            {
                cboServers.Visibility = Visibility.Hidden;
                return;     // empty list
            }

            cboServers.DisplayMemberPath = "Caption";
            cboServers.ItemsSource = cboServers.DataContext as IEnumerable<object>;

            // http://www.dotnetperls.com/combobox-wpf
            cboServers.SelectionChanged += (s,e) => {
                var selected = cboServers.SelectedItem as ServerDBName;
                ServerList.ComboSelect(selected, dataContext);
            };

            var first = Enumerable.First(cboServers.DataContext as ObservableCollection<ServerDBName>);
            cboServers.Text = first.Caption;

            if (!string.IsNullOrWhiteSpace(first.DbName))
               Guard.Check(dataContext.GetValue<string>("dbName").Equals(first.DbName), "SelectionChanged error");
        }
    }


}

// <ComboBox
//ItemsSource="{Binding Countries, Mode=OneWay}"
//DisplayMemberPath="Name"
//SelectedItem="{Binding SelectedDestinationCountry}" />
