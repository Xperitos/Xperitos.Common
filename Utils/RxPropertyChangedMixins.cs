using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using ReactiveUI;

namespace Xperitos.Common.Utils
{
    public static class RxPropertyChangedMixins
    {
        /// <summary>
        /// Returns a stream of changes for the specified property (whenever it changes).
        /// </summary>
        /// <param name="stream">The events stream</param>
        /// <param name="property">Property to obtain</param>
        /// <returns></returns>
        public static IObservable<KeyValuePair<TSender, TRet>> ForProperty<TSender, TRet>(this System.IObservable<IReactivePropertyChangedEventArgs<TSender>> stream, Expression<Func<TSender, TRet>> property)
        {
            string propertyName = RxObjectMixins.PropertyName(property);
            var getter = property.Compile();

            return stream
                .Where(e => e.PropertyName == propertyName)
                .Select(e => new KeyValuePair<TSender, TRet>(e.Sender, getter(e.Sender)));
        }

        /// <summary>
        /// Returns a stream of events when the specified properties changes.
        /// </summary>
        /// <param name="stream">The events stream</param>
        public static IObservable<IReactivePropertyChangedEventArgs<TSender>> ForProperties<TSender, TRet1, TRet2>(this System.IObservable<IReactivePropertyChangedEventArgs<TSender>> stream,
            Expression<Func<TSender, TRet1>> property1,
            Expression<Func<TSender, TRet2>> property2)
        {
            return ForProperties(stream, RxObjectMixins.PropertyName(property1), RxObjectMixins.PropertyName(property2));
        }

        /// <summary>
        /// Returns a stream of events when the specified properties changes.
        /// </summary>
        /// <param name="stream">The events stream</param>
        public static IObservable<IReactivePropertyChangedEventArgs<TSender>> ForProperties<TSender, TRet1, TRet2, TRet3>(this System.IObservable<IReactivePropertyChangedEventArgs<TSender>> stream,
            Expression<Func<TSender, TRet1>> property1,
            Expression<Func<TSender, TRet2>> property2,
            Expression<Func<TSender, TRet3>> property3)
        {
            return ForProperties(stream, RxObjectMixins.PropertyName(property1), RxObjectMixins.PropertyName(property2), RxObjectMixins.PropertyName(property3));
        }

        /// <summary>
        /// Returns a stream of events when the specified properties changes.
        /// </summary>
        /// <param name="stream">The events stream</param>
        public static IObservable<IReactivePropertyChangedEventArgs<TSender>> ForProperties<TSender, TRet1, TRet2, TRet3, TRet4>(this System.IObservable<IReactivePropertyChangedEventArgs<TSender>> stream,
            Expression<Func<TSender, TRet1>> property1,
            Expression<Func<TSender, TRet2>> property2,
            Expression<Func<TSender, TRet3>> property3,
            Expression<Func<TSender, TRet4>> property4)
        {
            return ForProperties(stream, RxObjectMixins.PropertyName(property1), RxObjectMixins.PropertyName(property2), RxObjectMixins.PropertyName(property3), RxObjectMixins.PropertyName(property4));
        }

        private static IObservable<IReactivePropertyChangedEventArgs<TSender>> ForProperties<TSender>(System.IObservable<IReactivePropertyChangedEventArgs<TSender>> stream, params string[] propertyNames)
        {
            return stream.Where(e => propertyNames.Contains(e.PropertyName));
        }
    }
}