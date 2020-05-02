using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Dotnet.Reflection
{
    // Object values Reflection Utilities, most of them [NullSafe]

    public static class Utils
    {
#if !NETCORE21 // NET451 || WPF || WEB
        public static IEnumerable<PropertyDescriptor> Properties(this object obj)
        {
            return ReflectionCache.GetProperties(obj);
        }

        public static XElement AsXml(this object obj, string name = null)
        {
            IEnumerable<PropertyDescriptor> prop = Utils.Properties(obj);
            XElement result = new XElement(name ?? obj.GetType().Name);
            foreach (PropertyDescriptor desc in prop)
            {
                object propValue = Utils.GetPropertyValue(obj, desc.Name);
                if (propValue != null)
                    result.Add(new XAttribute(desc.Name, propValue));
            }
            return result;
        }
#endif

        [NullSafe]
        public static ICollection<T> AddRange<T>(this ICollection<T> obj, IEnumerator data)
        {
            if (obj == null || data == null)
                return obj;

            // First record
            if (data.Current is T)
                obj.Add((T)data.Current);

            while (data.MoveNext())
            {
                if (data.Current is T)
                    obj.Add((T)data.Current);
            }

            return obj; // fluent return
        }

        // DictionaryExtensions
        // http://www.codeproject.com/Articles/1027959/Top-Underutilized-Features-of-NET-Part

        #region Values, Properties, Contains

        [NullSafe]
        public static T ValueOrDefault<TKey, T>(this Dictionary<TKey, T> dict, TKey key)
        {
            T result;
            return dict != null && dict.TryGetValue(key, out result) ? result : default(T);
        }

        [NullSafe]
        public static T TryGetSafe<TKey, T>(this Dictionary<TKey, T> dict, TKey key)
        {
            T result;
            return dict != null && dict.TryGetValue(key, out result) ? result : default(T);
        }

        [NullSafe]
        public static T IsNullCase<T>(this object obj, T value2 = null) where T : class, new()
        {
            T result = (T)obj ?? value2;
            if (result == null)
                return default(T);
            return result;
        }

        [NullSafe]
        public static bool IsEmpty<T>(this IEnumerable<T> obj) where T : class
        {
            if (obj == null)
                return true;

            bool result = false;
            var numerat = obj.GetEnumerator();
            if (!numerat.MoveNext() || numerat.Current == null)
                result = true;  // is empty

            if (numerat is IDisposable)
                (numerat as IDisposable).Dispose();

            return result;
        }

        [NullSafe]
        // is numerator is empty
        public static bool IsEmptyNumerat(this IEnumerator obj)
        {
            if (obj == null)
                return true;

            bool result = false;
            var numerat = obj;
            if (!numerat.MoveNext() || numerat.Current == null)
                result = true;

            //if (numerat is IDisposable)
            //    (numerat as IDisposable).Dispose();
            return result;
        }

        [NullSafe]
        public static bool IsEmpty<T>(this T[] obj)
        {
            if (obj == null || obj.Length == 0)
                return true;

            var value0 = obj[0];
            if (obj.Length == 1 && value0 == null)
                return true;

            return false;
        }

        [NullSafe]
        // lambda get property value, not fast, but null safe
        public static T GetSafe<TObj, T>(this TObj obj, Func<TObj, T> get, T ifNullValue = default(T))
        {
            if (obj == null) 
               return ifNullValue;
            
            T value = get(obj);
            return value.Equals(null) ? ifNullValue : value;
        }

        [NullSafe]
        public static bool Contains(this object obj, params object[] array)
        {
            if (obj == null || array == null || array.Length == 0) return false;

            foreach (var value in array)
                if (value != null && value.Equals(obj))
                    return true;

            return false;
        }

        [NullSafe]
        public static bool InList(this object obj, IEnumerable array)
        {
            if (obj == null) return false;

            foreach (var value in array)
                if (value != null && value.Equals(obj))
                    return true;

            return false;
        }

        [NullSafe]
        public static bool InList(this object obj, IEnumerator array)
        {
            if (array == null) return false;

            while (array.MoveNext())
                if (array.Current != null && array.Current == obj)
                    return true;

            return false;
        }

        public static bool InList<T>(this object obj, params object[] array)
        {
            foreach (var value in array)
                if (value != null && value is T
                    && value == obj)
                    return true;

            return false;
        }

        #endregion

        #region Property values

        [NotNullSafe]
        public static object GetPropertyValue(this object obj, string property)
        {
            Guard.Check(!(obj is ExpandoObject));

            object value = null;
#if !NETCORE21 // NET451 || WPF || WEB
            ReflectionCache.TryToExtractValueFromDescriptor(obj, property, out value);
#endif
            return value;
        }

        [NullSafe]
        public static void SetPropertyValue(this object obj, string propertyName, object propertyValue)
        {
            if (propertyValue == null || obj == null)
                return;

            Guard.Check(!(obj is ExpandoObject));

            PropertyInfo propertyInfo = GetProperty(obj, propertyName);
            if (propertyInfo == null)
                throw new ArgumentException(string.Format(
                    "An error has occurred setting property with the '{0}' name does not exist in {1}.",
                    propertyName, obj.GetType().Name));

            object value = null;
            if (propertyInfo.PropertyType == typeof(string))
            {
                value = propertyValue as string;
            }
            else if (propertyValue as string != null)
            {
#if !NETCORE21 // NET451 || WPF || WEB
                TypeConverter typeConverter = GetPropertyTypeConverter(propertyInfo);
                if (typeConverter != null)
                    value = typeConverter.ConvertFrom(null, CultureInfo.CurrentCulture, propertyValue);
                if (propertyInfo.PropertyType.IsEnum)
                    value = Enum.Parse(propertyInfo.PropertyType, propertyValue as string);

                if (value == null)
                {
                    typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                    // Converter &lt; transforming
                    value = typeConverter.ConvertFromString(null, CultureInfo.GetCultureInfo("en-US"),
                                    propertyValue as string);
                }
#endif
            }
            else
            {
                if (propertyInfo.PropertyType == typeof(Int32?))
                    value = Convert.ToInt32(propertyValue);
                else if (propertyInfo.PropertyType == typeof(decimal?))
                    value = Convert.ToDecimal(propertyValue);
                else if (propertyInfo.PropertyType == typeof(double?))
                    value = Convert.ToDouble(propertyValue);
                else
                    value = propertyValue;
            }

            // http://stackoverflow.com/questions/622664/what-is-immutability-and-why-should-i-worry-about-it
            if (propertyInfo.CanWrite)
                propertyInfo.SetValue(obj, value, null);
        }

#if !NETCORE21 // NET451 || WPF || WEB
        private static object TryGetPropertyValue(object obj, string propertyName, out bool success)
        {
            object value = null;
            success = ReflectionCache.TryToExtractValueFromDescriptor(obj, propertyName, out value);
            return value;
        }

        public static TypeConverter GetPropertyTypeConverter(PropertyInfo propertyInfo)
        {
            foreach (object attribute in propertyInfo.GetCustomAttributes(typeof(TypeConverterAttribute), false))
            {
                TypeConverterAttribute attr = attribute as TypeConverterAttribute;
                if (!attr.IsDefaultAttribute())
                {
                    try
                    {
                        var converter = Activator.CreateInstance(Type.GetType(attr.ConverterTypeName))
                                        as TypeConverter;
                        return converter;
                    }
                    catch { }
                }
            }
            return null;
        }
#endif

        #endregion

        #region DX Property methods

        /* private static object GetCompositePropertyValue(object obj, string propertyName)
        {
            List<string> propertyNames = new List<string>(propertyName.Split(PropertyPathSeparator));
            propertyNames.Remove(string.Empty);
            object result = obj;
            foreach (string propName in propertyNames)
                result = GetPropertyValue(result, propName);
            return result;
        }  */

#if !NETCORE21 // NET451 || WPF || WEB
        public static PropertyInfo GetProperty(object obj, string propertyName)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                    return property;
            }
            return null;
        }

        public static PropertyDescriptor GetPropertyDesc(object obj, string propertyName)
        {
            IEnumerable<PropertyDescriptor> prop = ReflectionCache.GetProperties(obj);
            return 
                Enumerable.First(
                Enumerable.Where(prop, (el) => el.Name == propertyName));
        }
#endif

        #endregion
    }


    public class NullSafeAttribute : Attribute
    {
    }

    public class NotNullSafeAttribute : Attribute
    {
    }

}
