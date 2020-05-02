using System;
using System.Reflection;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CoreLib
{
    [JsonConverter(typeof(StringEnumConverter))]
    public class TestJson
    {
        public XDocument Doc { get; set; }

        public void Test1()
        {
            var md = this.GetType().GetMethod("Test1");
            // .nuget\packages\System.Reflection.TypeExtensions\4.0.0\ref\dotnet\System.Reflection.TypeExtensions.dll
        }
    }
}
