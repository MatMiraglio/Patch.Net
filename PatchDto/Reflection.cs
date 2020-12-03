using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Patch.Net
{
    public static class Reflection
    {

        public static object GetValue<T>(string propertyName, T from)
        {
            return typeof(T)
                .GetProperty(propertyName)
                .GetValue(from);
        }

        public static void Assign(object value, string propertyName, object to)
        {
            to.GetType()
              .GetProperty(propertyName)
              .SetValue(to, value);
        }
    }
}