using System;
using System.Linq.Expressions;

namespace Xperitos.Common.Utils
{
    /// <summary>
    /// Convinence class to obtain a contant property name
    /// </summary>
    public static class NameFor<T>
    {
        public static string Property<TRet>(Expression<Func<T, TRet>> property)
        {
            var propExpr = property.Body as MemberExpression;
            if (propExpr == null)
                throw new ArgumentException("Property expression must be of the form 'x => x.SomeProperty'");

            return propExpr.Member.Name;
        }
    }
}