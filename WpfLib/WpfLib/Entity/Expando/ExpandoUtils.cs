using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    public static class ExpandoUtils
    {
        public static ICollection<string> Keys(this ExpandoObject obj)
        {
            return (obj as IDictionary<string, object>).Keys;
        }
    }
}
