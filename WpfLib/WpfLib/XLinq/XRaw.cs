using System.Linq;

namespace Ai.XLinq
{
    public class XRaw : System.Xml.Linq.XText
    {
        public XRaw(string text) : base(text) { }
        public XRaw(System.Xml.Linq.XText text) : base(text) { }

        public override void WriteTo(System.Xml.XmlWriter writer)
        {
            writer.WriteRaw(this.Value);
        }
    }
}
