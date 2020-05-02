using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ai.Reflection
{
    public static class Utils
    {
        public static object GetPropertyValue(this object obj, string property)
        {
            object value = null;
            // bool success = 
			TryToExtractValueFromDescriptor(obj, property, out value);
            return value;
        }

        public static bool IsPropertyExist(object obj, string propertyName)
        {
            PropertyDescriptor desc = propertyCache.GetPropertyDescriptor(obj, propertyName);
            return desc != null;
        }

        public static object GetPropertyValue(System.ComponentModel.PropertyDescriptorCollection properties, object obj, string property)
        {
            var value = properties.GetPropertyValue(property);
            return value;
        }

        public static bool TryToGetPropertyValue(this object obj, string property, out object value)
        {
            return TryToExtractValueFromDescriptor(obj, property, out value);
        }

        #region Values Get, Property Cache

        private static PropertyCache propertyCache;
        static Utils() { propertyCache = new PropertyCache(); }

        private class PropertyCache
        {
            public struct TypePropertyPair
            {
                public Type type;
                public string propertyName;
            }

            static Dictionary<TypePropertyPair, PropertyDescriptor> cache = new Dictionary<TypePropertyPair, PropertyDescriptor>();

            public PropertyDescriptor GetPropertyDescriptor(object source, string propertyName)
            {
                TypePropertyPair key = new TypePropertyPair() { type = source != null ? source.GetType() : null, propertyName = propertyName };
                if (key.type == null || key.propertyName == null)
                    return null;
                if (cache.ContainsKey(key))
                    return cache[key];
                else
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(source);
                    PropertyDescriptor descriptor = properties == null ? null : properties.Find(propertyName, ignoreCase: true);
                    if (descriptor == null) return null;

                    lock (cache)
                    {
                        if (!cache.ContainsKey(key))
                            cache.Add(key, descriptor);
                    }
                    return descriptor;
                }
            }
        }

        private static bool TryToExtractValueFromDescriptor(object source, string propertyName, out object value)
        {
            var descriptor = propertyCache.GetPropertyDescriptor(source, propertyName);
            if (descriptor != null)
            {
                value = descriptor.GetValue(source);
                return true;
            }

            value = null;
            return false;
        }
       
        #endregion

        public static void SetPropertyValue(this object obj, string propertyName, object propertyValue)
        {
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
            }

            propertyInfo.SetValue(obj, value, null);
        }

        private static object TryGetPropertyValue(object obj, string propertyName, out bool success)
        {
            object value = null;
            success = TryToExtractValueFromDescriptor(obj, propertyName, out value);
            return value;
        }

        private static TypeConverter GetPropertyTypeConverter(System.Reflection.PropertyInfo propertyInfo)
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

        public static PropertyDescriptorCollection GetProperties(object obj)
        {
            // PropertyInfo[]
            // PropertyDescriptorCollection(PropertyDescriptor[] properties);
            // var infoArray = obj.GetType().GetProperties();

            return TypeDescriptor.GetProperties(obj.GetType());
        }

        #region Property methods

        private static PropertyInfo GetProperty(object obj, string propertyName)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                    return property;
            }
            return null;
        }

        #endregion
    }
}
