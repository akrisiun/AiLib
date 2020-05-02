using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using ExpandoKeyValue = System.Collections.Generic.KeyValuePair<string, object>;

namespace Dotnet.Reflection
{
    public static class ExpandoArray
    {
        public static object[] ToArray(this ExpandoObject source, int len)
        {
            object[] array = new object[len];
            int index = -1;
            var numerator = System.Linq.Enumerable.AsEnumerable<ExpandoKeyValue>(source);
            foreach (ExpandoKeyValue item in numerator)
                array[index++] = item.Value;

            return array;
        }

        public static IEnumerable<ExpandoKeyValue> AsKeyValue(this ExpandoObject source)
        {
            return System.Linq.Enumerable.AsEnumerable<ExpandoKeyValue>(source);
        }

        public static bool IsEmpty(object[] array) => StringConvert.IsArrayEmpty(array);

        public static ExpandoObject AsExpando(this object[] array)
        {
            ICollection<ExpandoKeyValue> result = new ExpandoObject() as ICollection<KeyValuePair<string, object>>;
            if (array == null || array.Length == 0)
                return result as ExpandoObject;
            var numerator = array.GetEnumerator();
            int index = -1;
            while (numerator.MoveNext())
            {
                index++;
                result.Add(new ExpandoKeyValue((index + 1).ToString(), array[index]));
            }

            return result as ExpandoObject;
        }

        public static ExpandoObject AsExpando(this object[] array, ExpandoObject header, object emptyCase = null)
        {
            IEnumerable<ExpandoKeyValue> numerable = System.Linq.Enumerable.AsEnumerable<ExpandoKeyValue>(header);
            int index = -1;

            ICollection<ExpandoKeyValue> result = new ExpandoObject();
            var numerator = numerable?.GetEnumerator();
            if (numerator == null)
                return result as ExpandoObject;

            while (numerator.MoveNext())
            {
                // Index was outside the bounds of the array.
                index++;
                var value = index < array.Length ? array[index] : emptyCase;
                result.Add(new ExpandoKeyValue(numerator.Current.Key, value));
            }

            return result as ExpandoObject;
        }

        public static IEnumerable<ExpandoObject> CastByHeader(this IEnumerable<object[]> arrayNumerator, ExpandoObject header)
        {
            if (arrayNumerator == null)
                yield break;

            var numerator = arrayNumerator.GetEnumerator();
            while (numerator.MoveNext())
            {
                var data = numerator.Current as object[];
                yield return ExpandoArray.AsExpando(data, header);
            }
        }
    }

}
