using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Xperitos.Common.Utils
{
    public static class ObservableMixins
    {
        /// <summary>
        /// Convert to a unified return value.
        /// </summary>
        /// <param name="generateInitialValue">When true, an initial value will be merged with the returned observable</param>
        /// <returns>A unit observable</returns>
        public static IObservable<Unit> AsUnit<T>(this IObservable<T> obj, bool generateInitialValue = false)
        {
            if (generateInitialValue)
                return obj
                    .Select(v => Unit.Default)
                    .Merge(Observable.Return(Unit.Default));

            return obj.Select(v => Unit.Default);
        }
    }
}