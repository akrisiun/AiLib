using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ai.Entity.Linq
{

    public class TestMapping : System.Data.Linq.Mapping.DataAttribute
    // sealed class ColumnAttribute : System.Data.Linq.Mapping.DataAttribute
    // Member of System.Data.Linq.Mapping
    { 
    
    }


    public interface IAttributeResolver
    {
        string Resolve(DataAttribute attribute);
    }

    // var lastNameMaxLength = AttributeResolver.MaxLength<Users>(u => u.LastName);
    public class AttributeResolver
    {
        public string Resolve(DataAttribute attribute)
        {
            return "";
        }

        // public int MaxLength<T>(Expression<Func<T, object>> propertyExpression)
        // {
            // Do the good stuff to get the PropertyInfo from the Expression...
            // Then get the attribute from the PropertyInfo
            // Then read the value from the attribute
        // }
    }

    // I've found this class helpful in resolving properties from Expressions:
    public class LinqTypeHelper
    {
        private static PropertyInfo GetPropertyInternal(LambdaExpression p)
        {
            MemberExpression memberExpression;

            if (p.Body is UnaryExpression)
            {
                UnaryExpression ue = (UnaryExpression)p.Body;
                memberExpression = (MemberExpression)ue.Operand;
            }
            else
            {
                memberExpression = (MemberExpression)p.Body;
            }
            return (PropertyInfo)(memberExpression).Member;
        }

        public static PropertyInfo GetProperty<TObject>(Expression<Func<TObject, object>> p)
        {
            return GetPropertyInternal(p);
        }
    }

}
