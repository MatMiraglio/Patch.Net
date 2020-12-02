using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Patch.Net
{
    public static class Helper
    {
        public static string GetPropertyName<TSource>(Expression<Func<TSource, object>> propertyLambda)
        {
            LambdaExpression lambda = propertyLambda;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression expression)
            {
                var unaryExpression = expression;
                memberExpression = (MemberExpression)(unaryExpression.Operand);
            }
            else
            {
                memberExpression = (MemberExpression)(lambda.Body);
            }

            return ((PropertyInfo)memberExpression.Member).Name;
        }

        public static object GetValue<T>(string propertyName, T from)
        {
            return typeof(T)
                .GetProperty(propertyName)?
                .GetValue(from, null);
        }

        public static void Assign(object value, string propertyName, object to)
        {
            to.GetType()
              .GetProperty(propertyName)?
              .SetValue(to, value, null);
        }
    }
}