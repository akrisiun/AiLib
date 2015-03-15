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

namespace Ai.Wpf.Entity
{
    /// <summary>
    ///     xmlns:MyNamespace="clr-namespace:WExec.Entity;assembly=WExec.Entity"
    /// You will also need to add a project reference from the project where the XAML file lives
    /// 
    /// Step 2: Go ahead and use your control in the XAML file.
    ///     <MyNamespace:CustomControl1/>
    /// </summary>
    public class ToolLine : Control
    {
        static ToolLine()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolLine), new FrameworkPropertyMetadata(typeof(ToolLine)));
        }

        public static string urlXml { get { return "Wpf.Entity"; } }

    }
}
