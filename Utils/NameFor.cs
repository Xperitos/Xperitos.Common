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
            return RxObjectMixins.PropertyName(property);
        }
    }
}