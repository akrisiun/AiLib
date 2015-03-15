using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlAgilityPack
{
    public class XHtmlEntity
    {

        public static readonly Dictionary<int, string> _entityName;
        public static readonly Dictionary<string, int> _entityValue;

        public enum AttributeState
        {
            OutSide,
            InsideAttr,
            InsideAttrValue
        }

        static XHtmlEntity()
        {
            _entityName = HtmlEntity._entityName;
            _entityValue = HtmlEntity._entityValue;

            _entityValue.Remove("quot"); // , 34); // quotation mark = APL quote, U+0022 ISOnum 
            _entityName.Remove(34);      // , "quot");
            _entityValue.Remove("amp");  // , 38); // ampersand, U+0026 ISOnum 
            _entityName.Remove(38);      // , "amp");
            _entityValue.Remove("lt");   //  60); // less-than sign, U+003C ISOnum 
            _entityName.Remove(60);      // lt");
            _entityValue.Remove("gt");   //  62); // greater-than sign, U+003E ISOnum 
            _entityName.Remove(62);      // gt");

            // http://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references
            // _entityValue.Add("nbsp", 160); // no-break space = non-breaking space, U+00A0 ISOnum 
            // _entityName.Add(160, "nbsp");
        }

        public static string XDeEntitizeStyle(string text, Func<string, string, string> parseAttr)
        {
            var splitResult = Ai.XHtml.SplitPos.Split(text, new[] { '<', '>' });
            StringBuilder writer = new StringBuilder();

            string prevItem = null;
            bool isAttr = false;
            foreach (var item in splitResult.SplitWithBegin)
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                isAttr = item.StartsWith("<");
                if (isAttr)
                {
                    string itemParse = (parseAttr != null) ? parseAttr(item, prevItem) : item;
                    if (itemParse.Contains("&quot;"))
                        itemParse = itemParse.Replace("&quot;", "\'");

                    writer.Append(itemParse);
                    prevItem = itemParse;
                }
                else
                {
                    writer.Append(item);
                    prevItem = item;
                }

                //  isAttr = !isAttr && prevItem.StartsWith("<") && !prevItem.EndsWith(">")
                //      && !prevItem.EndsWith("/");
            }

            return XDeEntitizeText(writer.ToString(), withQuot: true);
        }

        // Note a few special case in this function:
        //     If there is no separator in the string, a string array which only contains 
        //     the original string will be returned regardless of the count. 

        public static string Parse(string html)
        {
            var doc = HtmlDocument.Xhtml(html);
            return doc.DocumentNode.InnerHtml;
        }

        public static string XDeEntitizeText(string text, bool withQuot = false)
        {
            if (text == null)
                return null;

            if (text.Length == 0)
                return text;

            var sb = new StringBuilder(text.Length);
            var state = HtmlEntity.ParseState.Text;
            var entity = new StringBuilder(10);

            for (var i = 0; i < text.Length; i++)
            {
                switch (state)
                {
                    case HtmlEntity.ParseState.Text:
                        switch (text[i])
                        {
                            case '&':
                                state = HtmlEntity.ParseState.EntityStart;
                                break;

                            default:
                                sb.Append(text[i]);
                                break;
                        }
                        break;

                    #region Entity
                    case HtmlEntity.ParseState.EntityStart:
                        switch (text[i])
                        {
                            case ';':
                                if (entity.Length == 0)
                                {
                                    sb.Append("&;");
                                }
                                else
                                {
                                    if (entity[0] == '#')
                                    {
                                        var e = entity.ToString();
                                        try
                                        {
                                            var codeStr = e.Substring(1).Trim().ToLower();
                                            int fromBase;
                                            if (codeStr.StartsWith("x"))
                                            {
                                                fromBase = 16;
                                                codeStr = codeStr.Substring(1);
                                            }
                                            else
                                            {
                                                fromBase = 10;
                                            }
                                            var code = Convert.ToInt32(codeStr, fromBase);
                                            sb.Append(Convert.ToChar(code));
                                        }
                                        catch
                                        {
                                            sb.Append("&#" + e + ";");
                                        }
                                    }
                                    else
                                    {
                                        // named entity?
                                        int code;
                                        int? o = _entityValue.ContainsKey(entity.ToString())
                                                 ? _entityValue[entity.ToString()] as int? : null;

                                        if (o == null)
                                        {
                                            string strEntity = entity.ToString();
                                            if (strEntity != "quot" || withQuot)
                                                sb.Append("&" + strEntity + ";");

                                        }
                                        else
                                        {
                                            // we found one
                                            code = o.Value;
                                            sb.Append(Convert.ToChar(code));
                                        }
                                    }
                                    entity.Remove(0, entity.Length);
                                }
                                state = HtmlEntity.ParseState.Text;
                                break;

                            case '&':
                                // new entity start without end, it was not an entity...
                                sb.Append("&" + entity);
                                entity.Remove(0, entity.Length);
                                break;

                            default:
                                entity.Append(text[i]);
                                if (entity.Length > HtmlEntity._maxEntitySize)
                                {
                                    // unknown stuff, just don't touch it
                                    state = HtmlEntity.ParseState.Text;
                                    sb.Append("&" + entity);
                                    entity.Remove(0, entity.Length);
                                }
                                break;
                        }
                        break;
                    #endregion
                }
            }

            // finish the work
            if (state == HtmlEntity.ParseState.EntityStart)
            {
                sb.Append("&" + entity);
            }

            return sb.ToString();
        }

    }


}
