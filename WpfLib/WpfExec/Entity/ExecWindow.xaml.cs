using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfExec.Entity;

namespace WpfExec
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ExecWindow : Window
    {
        public bool isFirst = false;
        public ExecWindow()
        {
            InitializeComponent();
            ExecBind.Init(this);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                Uri iconUri = new Uri("pack://application:,,,/app.ico", UriKind.RelativeOrAbsolute);
                this.Icon = BitmapFrame.Create(iconUri);
                // Ai.Wpf.WpfTitleHelper.OnInitHideIcon(this, e);
            } catch { }
            base.OnSourceInitialized(e);
        }
    }
}
