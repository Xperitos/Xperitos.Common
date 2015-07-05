using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class TaskObservableMixins
    {
        /// <summary>
        /// Extract the result of a task as a stream of results.
        /// </summary>
        public static IObservable<T> Unwrap<T>(this IObservable<Task<T>> observable)
        {
            return observable.SelectMany(v => TaskObservableExtensions.ToObservable<T>(v));
        }
    }
}