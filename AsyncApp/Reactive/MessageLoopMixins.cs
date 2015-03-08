using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xperitos.Common.AsyncApp.Reactive
{
    public static class MessageLoopMixins
    {
        /// <summary>
        /// Wraps the source sequence in order to run its observer callbacks on the specified message loop.
        /// </summary>
        public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, MessageLoop messageLoop)
        {
            return source.ObserveOn(messageLoop.SyncContext);
        }

        /// <summary>
        /// Wraps the source sequence in order to run its observer callbacks on the specified message loop.
        /// </summary>
        public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, MessageLoopScheduler scheduler)
        {
            return source.ObserveOn(scheduler.MessageLoop);
        }

        /// <summary>
        /// Wraps the source sequence in order to run its observer callbacks on the message loop associated with the current thread.
        /// </summary>
        public static IObservable<TSource> ObserveOnMessageLoop<TSource>(this IObservable<TSource> source)
        {
            return source.ObserveOn(MessageLoop.Current.SyncContext);
        }

        /// <summary>
        /// Wraps the source sequence in order to run its subscription and unsubscription logic on the specified message loop.
        /// </summary>
        /// <remarks>
        /// Only the side-effects of subscribing to the source sequence and disposing subscriptions to the source sequence are run on the specified dispatcher.
        ///             In order to invoke observer callbacks on the specified dispatcher, e.g. to render results in a control, use <see cref="M:System.Reactive.Linq.DispatcherObservable.ObserveOn``1(System.IObservable{``0},System.Windows.Threading.Dispatcher)"/>.
        /// 
        /// </remarks>
        public static IObservable<TSource> SubscribeOn<TSource>(this IObservable<TSource> source, MessageLoop messageLoop)
        {
            return source.SubscribeOn(messageLoop.SyncContext);
        }

        /// <summary>
        /// Wraps the source sequence in order to run its subscription and unsubscription logic on the specified message loop.
        /// </summary>
        /// <remarks>
        /// Only the side-effects of subscribing to the source sequence and disposing subscriptions to the source sequence are run on the specified dispatcher.
        ///             In order to invoke observer callbacks on the specified dispatcher, e.g. to render results in a control, use <see cref="M:System.Reactive.Linq.DispatcherObservable.ObserveOn``1(System.IObservable{``0},System.Windows.Threading.Dispatcher)"/>.
        /// </remarks>
        public static IObservable<TSource> SubscribeOn<TSource>(this IObservable<TSource> source, MessageLoopScheduler scheduler)
        {
            return source.SubscribeOn(scheduler.MessageLoop);
        }

        /// <summary>
        /// Wraps the source sequence in order to run its subscription and unsubscription logic on the mesage loop associated with the current thread.
        /// </summary>
        /// <remarks>
        /// Only the side-effects of subscribing to the source sequence and disposing subscriptions to the source sequence are run on the dispatcher associated with the current thread.
        ///             In order to invoke observer callbacks on the dispatcher associated with the current thread, e.g. to render results in a control, use <see cref="M:System.Reactive.Linq.DispatcherObservable.ObserveOnDispatcher``1(System.IObservable{``0})"/>.
        /// </remarks>
        public static IObservable<TSource> SubscribeOnMessageLoop<TSource>(this IObservable<TSource> source)
        {
            return source.SubscribeOn(MessageLoop.Current.SyncContext);
        }
    }
}
