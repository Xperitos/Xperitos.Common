using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace Xperitos.Common.Utils
{
    public static class RxObjectMixins
    {
        /// <summary>
        /// Return the property name (using reflection).
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TRet"></typeparam>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string PropertyName<TSender, TRet>(this TSender obj, Expression<Func<TSender, TRet>> property)
            where TSender : ReactiveObject
        {
            return PropertyName(property);
        }

        public static string PropertyName<TSender, TRet>(Expression<Func<TSender, TRet>> property)
        {
            var propExpr = property.Body as MemberExpression;
            if (propExpr == null)
                throw new ArgumentException("Property expression must be of the form 'x => x.SomeProperty'");

            return propExpr.Member.Name;
        }

        /// <summary>
        /// Return the property name (using CallerMemberName).
        /// </summary>
        public static string PropertyName(this ReactiveObject obj, [CallerMemberName] string propertyName = "")
        {
            return propertyName;
        }
    }
}
