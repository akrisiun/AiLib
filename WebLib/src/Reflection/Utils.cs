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
            bool success = ReflectionUtils.TryToGetPropertyValue(obj, property, out value);
            return value;
        }

        public static void SetPropertyValue(this object obj, string propertyName, object propertyValue)
        {
            // propertyValue = HttpUtility.HtmlDecode(propertyValue);
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
            success = ReflectionUtils.TryToGetPropertyValue(obj, propertyName, out value);
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
          
        private static bool IsUrlProperty(PropertyInfo propertyInfo)
        {
            object[] attributes = propertyInfo.GetCustomAttributes(typeof(System.Web.UI.UrlPropertyAttribute), false);
            return attributes.Length > 0;
        }

        /*
        private static object PrepareUrlPropertyValue(object value, string themeName)
        {
            return UrlPathHelper.PrepareUrlPath(value.ToString(), themeName);
        }
        private static void SetCompositePropertyValue(object obj, string propertyName, string propertyValue)
        {
            string[] simpleProperties = propertyName.Split(PropertyPathSeparator);
            object targetObject = obj;
            for (int i = 0; i < simpleProperties.Length; i++)
            {
                string currentPropertyName = simpleProperties[i];
                if (i == simpleProperties.Length - 1)
                    SetPropertyValue(targetObject, currentPropertyName, propertyValue);
                else
                    targetObject = GetPropertyValue(targetObject, currentPropertyName);
            }
        } */
        
        #endregion
    }
}
