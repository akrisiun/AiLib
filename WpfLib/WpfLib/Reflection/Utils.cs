using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ai.Reflection
{
    public static class ExpandoConvert
    {
        public static T Convert<T>(this ExpandoObject source, T result)
            where T : class
        {
            var ctor = result.GetType().GetConstructors().Single();

            IDictionary<string, object> dict = source;
            var parameters = ctor.GetParameters();
            var parameterValues = parameters.Select(p => dict[p.Name]).ToArray();

            return (T)ctor.Invoke(parameterValues);
        }

        public static ExpandoObject Muttable(this object obj)
        {
            IDictionary<string, object> result = new ExpandoObject();
            IEnumerable<PropertyDescriptor> listProperties = Utils.Properties(obj);
            
            foreach (PropertyDescriptor item in listProperties)
            {
                var prop = item.Name;
                result.Add(prop, item.GetValue(obj));
            }
            return result as ExpandoObject;
        }

        // Unsafe method
        public static object DynValue(this ExpandoObject obj, string key)
        {
            return (obj as IDictionary<string, object>)[key];
        }

        public static T DynValue<T>(this ExpandoObject obj, string key) where T : IConvertible
        {
            if (typeof(T).Equals(typeof(string)))           // most popular call
                // http://stackoverflow.com/questions/10387095/cast-int-to-generic-enum-in-c-sharp
                return (T)(object)System.Convert.ToString((obj as IDictionary<string, object>)[key]);

            // https://raw.githubusercontent.com/Microsoft/referencesource/master/mscorlib/system/convert.cs
            // var typeCode = (default(T) as IConvertible).GetTypeCode(); - failed
            T res = (T)System.Convert.ChangeType(
                  (obj as IDictionary<string, object>)[key] as IConvertible, typeof(T), provider: null);
            return res;
        }

        // Unsafe method
        public static ExpandoObject DynValueSet(this ExpandoObject obj, string key, object value)
        {
            (obj as IDictionary<string, object>)[key] = value;
            return obj;
        }

        // Safe method
        public static object SetValue<T>(this object obj, string propertyName, T propertyValue, Action<Exception> OnError = null)
            where T : IConvertible
        {
            if (OnError != null && string.IsNullOrWhiteSpace(propertyName))
            {
                OnError(new ArgumentException("no property propertyValue"));
                return obj;
            }
            Guard.Check(obj != null && !string.IsNullOrWhiteSpace(propertyName),
                        "error in SetValue property");

            if (obj is ExpandoObject)
            {
                (obj as IDictionary<string, object>)[propertyName] = propertyValue;
                return obj;
            }

            PropertyInfo pi = obj.GetType().GetProperty(propertyName);
            if (pi == null || !pi.CanWrite)
            {
                if (OnError != null)
                    OnError(new ArgumentException("no property " + propertyName));
                return obj;
            }

            pi.SetValue(obj,
                System.Convert.ChangeType(propertyValue, pi.PropertyType),
                null
            );
            return obj;
        }

        // Safe method
        public static T GetValue<T>(this object obj, string propertyName)
        // where T : IConvertible
        {
            if (obj is ExpandoObject || obj is IDictionary<string, object>)
            {
                var dict = obj as IDictionary<string, object>;
                object value = null;
                dict.TryGetValue(propertyName, out value);
                return (T)value;
            }
            
            PropertyDescriptor pi = ReflectionCache.GetProperty(obj, propertyName);
            if (pi == null)    //  || !pi.CanRead // .CanWrite
                return default(T);

            return (T)System.Convert.ChangeType(obj.GetPropertyValue(propertyName), pi.PropertyType);
        }

        public static T ChangeType<T>(object value)
        {
            return (T)ChangeTypeObj(value, typeof(T));
        }

        public static Type RemoveNullableFromType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        // https://raw.githubusercontent.com/Microsoft/referencesource/c697a4b9782dc8c85c02344a847cb68163702aa7/System.Web.DynamicData/DynamicData/Util/Misc.cs
        public static object ChangeTypeObj(this object value, Type type)
        {
            if (type is IConvertible)
            {
                var typeCode = (type as IConvertible).GetTypeCode();
                if (value == null && (typeCode == TypeCode.Empty || typeCode == TypeCode.String || typeCode == TypeCode.Object))
                {
                    return null;
                }
            }


            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (value == null)
            {
                if (TypeAllowsNull(type))
                {
                    return null;
                }
                return System.Convert.ChangeType(value, type, CultureInfo.CurrentCulture);
            }

            type = RemoveNullableFromType(type);

            if (value.GetType() == type)
            {
                return value;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(value.GetType()))
            {
                return converter.ConvertFrom(value);
            }

            TypeConverter otherConverter = TypeDescriptor.GetConverter(value.GetType());
            if (otherConverter.CanConvertTo(type))
            {
                return otherConverter.ConvertTo(value, type);
            }

            throw new InvalidOperationException(String.Format(
                  CultureInfo.CurrentCulture, "cannot convert to {0}", type.FullName));
            //                DynamicDataResources.Misc_CannotConvertType, value.GetType(), type));
        }

        internal static bool TypeAllowsNull(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null || !type.IsValueType;
        }

    }

    public static class Utils
    {
        public static IEnumerable<PropertyDescriptor> Properties(this object obj)
        {
            return ReflectionCache.GetProperties(obj);
        }

        public static object GetPropertyValue(this object obj, string property)
        {
            Guard.Check(!(obj is ExpandoObject));

            object value = null;
            // bool success = 
            ReflectionCache.TryToExtractValueFromDescriptor(obj, property, out value);
            return value;
        }

        public static void SetPropertyValue(this object obj, string propertyName, object propertyValue)
        {
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

            // http://stackoverflow.com/questions/622664/what-is-immutability-and-why-should-i-worry-about-it
            if (propertyInfo.CanWrite)
                propertyInfo.SetValue(obj, value, null);
        }

        private static object TryGetPropertyValue(object obj, string propertyName, out bool success)
        {
            object value = null;
            success = ReflectionCache.TryToExtractValueFromDescriptor(obj, propertyName, out value);
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
