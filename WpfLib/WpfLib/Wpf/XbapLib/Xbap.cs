using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ai.WpfLib.XbapLib
{
    public static class Xbap
    {
        public static bool IsXbap
        {
            get
            {
                //     Gets a value that specifies whether the current Windows Presentation Foundation
                //     (WPF) application is browser hosted.
                return
                    System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted; // && !OptionsXBAP.SuppressNotSupportedException;
            }
        }

        // object ClientSite { get; }
        
        //     A script object that provides access to the HTML window object, custom script
        //     functions, and global variables for the HTML page, if the XAML browser application
        //     (XBAP) is hosted in a frame; otherwise, null.
        // [Dynamic]
        //public static dynamic HostScript { get; }
        
        //     Gets the uniform resource identifier (URI) for the location from which a
        //     XAML browser application (XBAP) application was launched.
        //public static Uri Source { get; }

    }
}
