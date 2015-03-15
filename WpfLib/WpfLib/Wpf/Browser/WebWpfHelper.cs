using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using mshtml;
using System.Windows.Navigation;   // using MsHtml = mshtml;

// VisualFoxpro

namespace Ai.Wpf
{
    public class WebBrowserEvt
    {
        public WebBrowserEvt(WebBrowser browser)
        {
            browser.LoadCompleted += browser_LoadCompleted;
            browser.Navigated += browser_Navigated;
            browser.Navigate("about:blank");
            browser.Navigating += browser_Navigating;
        }

        void browser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
           // throw new NotImplementedException();
        }

        void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var browser = sender as WebBrowser;
            var doc = browser.Document as mshtml.IHTMLDocument3;

            // [DispId(-2147417084)] string outerHTML { get; set; }
            var body = doc.documentElement.outerHTML;
            //SetReady(browser, doc, )
        }

        void browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            
        }


        public void SetReady(WebBrowser browser, mshtml.HTMLDocument doc, Action<IHTMLEventObj> onReady)
        { 
            // var ready = new DHTMLEventHandler(doc);
            
            //// OnDocumentComplete
            //browser.read
            //// WebBrowserDocumentCompleted
            //// System.Windows.Forms.WebBrowserDocumentCompletedEventArgs
            //// A WebBrowserDocumentCompletedEventArgs that contains the event data.

            var handler = WebWpfHelper.Bind(browser, onReady, "onreadystatechange");
        }
    }


    //[ComSourceInterfaces("mshtml.HTMLTextContainerEvents")]
    //[Guid("3050F24A-98B5-11CF-BB82-00AA00BDCE0B")]
    //[TypeLibType(2)]
    //public class HTMLBodyClass : DispHTMLBody, HTMLBody, HTMLTextContainerEvents_Event, IHTMLElement, IHTMLElement2, IHTMLElement3, IHTMLElement4, IHTMLUniqueName, IHTMLDOMNode, IHTMLDOMNode2, IHTMLControlElement, IHTMLTextContainer, IHTMLBodyElement, IHTMLBodyElement2, HTMLTextContainerEvents2_Event
    //{

    public delegate void DocHTMLEvent(HTMLDocument doc, IHTMLEventObj e);
    public delegate void DHTMLEvent(IHTMLEventObj e); 

    // http://en.efreedom.net/Question/1-6754968/Add-Event-Listener-Button-Created-CSharp-IE-BHO
    // These attributes may be optional, depending on the project configuration.
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class DHTMLEventHandler : IDisposable
    {
        public DHTMLEvent Handler;
        public HTMLDocument Document { get; set; }
        public WebBrowser browser { get; set; }

        public DHTMLEventHandler(HTMLDocument doc)
        {
            this.Document = doc;
        }
        
        // The "target" parameter is an implementation detail.
        // [DispId(0)] public void Evt(object target, IHTMLEventObj evt) 
        //{
        //    if (Handler != null)
        //        Handler(this.Document, evt);
        //}

        [DispId(0)] public void Call()     
        {         
            IHTMLEventObj evt = Document.parentWindow.@event;
            Handler(evt);
        }  
         
        // public void BrowserEventHandler(IHTMLEventObj e)     {         try         {             
        //if (e.type == "click" && e.srcElement.id == "IDOfmyButton")             { 

        public void Dispose()
        {
            Handler = null;
            // throw new NotImplementedException();
        }
    }

    // C:\Program Files (x86)\Microsoft.NET\Primary Interop Assemblies\Microsoft.mshtml.dll
    public static class WebWpfHelper
    {
        public static WebBrowserEvt Prepare(this WebBrowser browser)
        {
            var helper = new WebBrowserEvt(browser);
            return helper;
        }
        public static void Prepare(this WebBrowser browser, Action<WebBrowser, mshtml.IHTMLElement> onReady)
        {
            var helper = new WebBrowserEvt(browser);
            if (onReady != null)
                browser.Navigated += new NavigatedEventHandler((s, e) =>
                    {
                        if (browser.Document != null)
                        {
                            var doc = browser.Document as mshtml.IHTMLDocument3;
                            if (doc != null)
                                onReady(browser, doc.documentElement);
                        }
                    });
        }

        // OnDocumentComplete method (also in same namespace as above [extra info for novices]):
        public static DHTMLEventHandler Bind(this WebBrowser browser, Action<IHTMLEventObj> call, string method = "onreadystatechange")
        {
            var doc = browser.Document as HTMLDocument;
            DHTMLEventHandler handler = new DHTMLEventHandler(doc);
            handler.browser = browser;
            handler.Handler += (e) => call(e);

            string @event = method;
            doc.attachEvent(@event, handler);
            return handler;
        }

        public static string outerHTML(this WebBrowser browser)
        {
            var doc = browser.Document as mshtml.IHTMLDocument3;
            string outerHTML = doc == null ? null : doc.documentElement.outerHTML;
            return outerHTML;
        }


        public static string GetHtml(this WebBrowser browser)
        {
           var data = browser.DataContext;
           var html = browser.Document as mshtml.HTMLDocument;
           
            // bool attachEvent(string @event, object pdisp);
           // var expando = html.ex
           var ready = html.readyState;
           var body = html.body.outerHTML;
           return body;

           //var state = html.readyState;
           //  HRESULT IHTMLDocument2::get_onreadystatechange(VARIANT *p);HRESULT IHTMLDocument2::put_onreadystatechange(VARIANT v);
           //var ready = html.onreadystatechange;
           // [DispId(-2147412087)] dynamic onreadystatechange { get; set; }
           // HTMLDocument readyState
           // System.Windows.Application.Current
           //var body = html.body.innerHTML;

           // htmlDoc.getElementsByTagName () or htmlDoc.getElementByID()
        }

        public static mshtml.HTMLDocument SetHtml(this WebBrowser browser, string html, string xpath = null)
        {
            var htmlDoc = browser.Document as mshtml.HTMLDocument;
            var state = htmlDoc.readyState;
            // System.Windows.Application.Current

            htmlDoc.body.innerHTML = html;
            return htmlDoc;
        }

        //  HtmlDocument document = (HtmlDocument)webNav.webBrowser1.Document;

        //  public  HTMLDocumentClass GetHtmlDocument( FileInfo f )
        //  {
        //     HTMLDocumentClass doc = null;
        //  try
        //  {
        //    doc = new HTMLDocumentClass();
        //    UCOMIPersistFile persistFile = (UCOMIPersistFile)doc;
        //    persistFile.Load( f.FullName, 0 );
        //    int start = Environment.TickCount;
        //    while( doc.readyState != "complete" )
        //   { 
        //      System.Windows.Forms.Application.DoEvents();
        //      if ( Environment.TickCount - start > 10000 )
        //      {
        //        throw new Exception( string.Format( "The document {0} timed out while loading", f.Name ) );
        //      }
        //    }
        //  }
    }

}
