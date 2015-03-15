using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Ai
{
    /// <summary>
    /// Translations.xml reader
    /// </summary>
    public static class Trans
    {
        public static string Tr(this string key)
        {
            if (string.IsNullOrWhiteSpace(key) || doc == null)
                return key;
            var list =  doc.Root.Elements();
            XElement node = list.Where<XElement>(
                phrase => phrase.Name == "phrase" && phrase.Elements("key").Any()
                          && phrase.Element("key").Value == key).FirstOrDefault();

#if DEBUG
            if (node == null)
            {
               var el = new XElement("phrase", new XElement("key", key));
               el.Add(new XElement("lt", key));
               el.Add(new XElement("en", key));
               doc.Root.Add(el);
               lock (lockObj)
               {
                   doc.Save(TransFile);
               }
            }
#endif
            var ret = key;
            if (node == null)
                return ret;
            try
            {
                if (node.Element(Lang) != null)
                    ret = node.Element(Lang).Value;
                else if (TrLang != null && node.Element(TrLang) != null)
                    ret = node.Element(TrLang).Value;
                // else ret = key;
            }
            catch
            {
                ret = "[lang=" + Lang ?? "??" + "]" + key;
            }
            
            return ret;
        }
        private static object lockObj;

        public static string TransFile { get; set; }
        public static string Lang { get; set; }
        public static string TrLang { get; set; }

        public static XDocument doc { get; set; }

        static Trans()
        {
            Lang = "lt";    // default language
            TrLang = ConfigurationManager.AppSettings.Get("tr.lang");

            TransFile = "Translate.xml";
            LoadXml();
            lockObj = "transLock";
        }

        public static void LoadXml()
        {
            var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, TransFile);
            Ai.Log.Write("Trans.LoadXml " + filePath);
            if (!File.Exists(filePath))
            {
                Ai.Log.Write("no file " + filePath);
                return;
            }

            doc = XDocument.Load(filePath);
            if (doc == null)
                return;
            TransFile = filePath;
            if (doc.Root.Attributes("lang").Any())
                Lang = doc.Root.Attribute("lang").Value;
        }

    }
}
