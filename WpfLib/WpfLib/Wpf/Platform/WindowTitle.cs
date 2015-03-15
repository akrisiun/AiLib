using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Ai.Wpf
{
    public class WindowTitle
    {
        public static Application App { get { return System.Windows.Application.Current; }}
        public static Window Window { get { return App.MainWindow; } }

        // http://stackoverflow.com/questions/1283006/changing-wpf-title-bar-background-color
        public static void MinimizeClick(object s, EventArgs e)
        {
            var w = // (s as Window) ?? 
                Window;
            w.WindowState = WindowState.Minimized;
        }

        public static void MaximizeClick(object s, EventArgs e)
        {
            var w = Window;
            if (w.WindowState == WindowState.Maximized)
                w.WindowState = WindowState.Minimized;
            else if (w.WindowState == WindowState.Normal)
                w.WindowState = WindowState.Maximized;
        }

        public static void MouseMove(object sender, MouseEventArgs e)
        {
            (sender as Window).DragMove();
        }
    
        public static void SetupTitle(Window w)
        {
            var app = App;
            ////For MouseDown -
            //app.MainWindow.DragMove();
        
            //// For Minimize Button -
            //app.MainWindow.WindowState = WindowState.Minimized;

            //    // For DoubleClick and MaximizeClick
            //if (app.MainWindow.WindowState == WindowState.Maximized)
            //{
            //    app.MainWindow.WindowState = WindowState.Normal;
            //}
            //else if (app.MainWindow.WindowState == WindowState.Normal)
            //{
            //    app.MainWindow.WindowState = WindowState.Maximized;
            //}

        }

    }
}
