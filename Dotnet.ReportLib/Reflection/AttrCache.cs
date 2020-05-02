#if true // NET471

using Dotnet.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dotnet.Reflection
{
    // https://stackoverflow.com/questions/44429382/how-to-get-attribute-value-in-c-sharp-for-the-class-property
    // https://stackoverflow.com/questions/18765022/c-sharp-getcustomattributes-from-class-name

    public interface IFieldAttribute
    {
        // .Value
        string Value {get; set; }
    }

    public class FieldCaption : Attribute, IFieldAttribute
    {
        public string Value {get; set; }

        public FieldCaption(string value) {
            Value = value;
        }
    }

    public class FieldLength : Attribute, IFieldAttribute
    {
        public string Value {get; set; }
        public int? Len {get; set; }

        public FieldLength(int len) {
            Len = len;
            Value = len.ToString();
        }
    }
    
    /// <summary>
    /// AttrCache<T>.Members - attribute value
    /// </summary>
    /// <typeparam name="TAttr"></typeparam>
    public class AttrCache<TObj, TAttr> where TAttr : Attribute, IFieldAttribute
    {
        public static readonly IReadOnlyDictionary<MemberInfo, string> Members = AttrCache.BuildDictionary<TObj, TAttr>();

    }

    public class AttrCache
    { 
        /// <summary>
        /// Attribute value of type
        /// </summary>
        /// <typeparam name="TObj">not null, if found attribute</typeparam>
        /// <returns></returns>
        public static object FieldValue<TObj, TAttr>() where TAttr : Attribute, IFieldAttribute
        { 
            Type member = typeof(TObj);
            var attr = member.GetCustomAttribute<TAttr>(true);

            if (attr as IFieldAttribute != null) {
                return (attr as IFieldAttribute).Value;
            }

            return null;
        }

        public static string FieldCaption<TObj>() => FieldValue<TObj, FieldCaption>() as string;
        public static int? FieldLength<TObj>() => FieldValue<TObj, FieldLength>() as int?;
        public static int FieldLengthZero<TObj>() => (FieldValue<TObj, FieldLength>() as int?) ?? 0;

        public static Dictionary<MemberInfo, string> BuildDictionary<T, TAttr>() where TAttr : Attribute
        {
            var dict = new Dictionary<MemberInfo, string>();

            var members = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property);

            foreach (MemberInfo member in members) {
                var attr = member.GetCustomAttribute<TAttr>(true);

                if (attr as IFieldAttribute != null
                    && (attr as IFieldAttribute).Value as string != null) 
                {
                    dict.Add(member, (attr as IFieldAttribute).Value as string);
                }
            }

            return dict;
        }
    }
} 
#endif