using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Configuration;

namespace Dotnet
{
#if !NET40CL // #if NET451 || WEB || NET46 || WPF || MONO || UNIX
#endif

    public static class AppConfig
    {
#if !NET40CL // NET451 || WEB || NET46 || NET45 || 
        public static NameValueCollection AppSettings
        { [DebuggerStepThrough] get { return ConfigurationManager.AppSettings; } }

        // #if $(OS)' != 'Windows_NT'
        public static ConnectionStringSettingsCollection ConnectionStrings
        { [DebuggerStepThrough] get { return ConfigurationManager.ConnectionStrings; } }
#endif

#if WEB || WPF

        private static WebCfg _web = null;
        public static WebCfg Web { get {  return _web ?? (_web = new WebCfg()); } }

        public class WebCfg
        {
            public T Server<T>(string key = "web.server") where T : class
            { return (T)ConfigurationManager.GetSection(key); }

            public T Config<T>(string key = "web.config") where T : class
            { return (T)ConfigurationManager.GetSection(key); }
        }
#endif

    }
}

namespace Dotnet.Entity
{
    public interface ILastError
    {
        Exception LastError { get; set; }
    }
}
