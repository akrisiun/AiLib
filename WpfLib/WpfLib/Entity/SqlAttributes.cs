using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class MaxLengthAttribute : System.Attribute // ValidationAttribute
    {
        private const int MaxAllowableLength = -1;

        public int Length { get; private set; }

        public MaxLengthAttribute(int length = -1)
        {
            this.Length = length;
        }

        #region Validation 
        public string ErrorMessageString { get; private set; }

        public bool IsValid(object value)
        {
            this.EnsureLegalLengths();
            if (value == null)
                return true;
            string str = value as string;
            int num = str == null ? ((Array)value).Length : str.Length;
            if (-1 != this.Length)
                return num <= this.Length;
            else
                return true;
        }

        public string FormatErrorMessage(string name)
        {
            return string.Format((IFormatProvider)CultureInfo.CurrentCulture, this.ErrorMessageString, new object[2]
              {
                (object) name,
                (object) this.Length
              });
        }

        private void EnsureLegalLengths()
        {
            if (this.Length == 0 || this.Length < -1)
                throw new InvalidOperationException(string.Format("MaxLengthAttribute_InvalidMaxLength"));
        }
        #endregion

    }
}
