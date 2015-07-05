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
            return observable.SelectMany(v => v.ToObservable());
        }

        /// <summary>
        /// Same as <see cref="Observable.Select{TSource,TResult}(System.IObservable{TSource},System.Func{TSource,TResult})"/> but for an async function.
        /// It automatically unwraps the task.
        /// </summary>
        public static IObservable<TResult> SelectAsync<TSource, TResult>(this IObservable<TSource> observable, Func<TSource, Task<TResult>> selector)
        {
            return observable.Select(selector).Unwrap();
        }

        /// <summary>
        /// Same as <see cref="Observable.Select{TSource,TResult}(System.IObservable{TSource},System.Func{TSource,TResult})"/> but for an async function.
        /// It automatically unwraps the task.
        /// </summary>
        public static IObservable<TResult> SelectAsync<TSource, TResult>(this IObservable<TSource> observable, Func<TSource, int, Task<TResult>> selector)
        {
            return observable.Select(selector).Unwrap();
        }
    }
}