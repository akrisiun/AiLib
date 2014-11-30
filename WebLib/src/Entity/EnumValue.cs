using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ai.Entity
{
    public class EnumValue : System.Attribute
    {
        private string _value;

        public EnumValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }


        /// <summary>
        /// Get string from Enum name 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Get(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            EnumValue[] attrs = fi.GetCustomAttributes(typeof(EnumValue), false) as EnumValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }

            return output;
        }

    }

}
