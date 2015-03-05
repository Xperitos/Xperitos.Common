using System;
using System.Reactive.Linq;

namespace Xperitos.Common.Utils
{
    public static class SkipNextMixins
    {
        /// <summary>
        /// Skips the elements on a sequence according to the given predicate.
        /// </summary>
        /// <param name="predicate">Return number of elements to skip</param>
        public static IObservable<T> SkipNext<T>(this IObservable<T> observable, Func<T, int> predicate)
        {
            var refCounted = observable.Select(v => new {V = v, SkipItems = predicate(v)}).Publish().RefCount();

            return Observable.Create<T>(
                observer =>
                {
                    int[] itemsToSkip = {0};
                    return refCounted.Subscribe(( v ) =>
                                                {
                                                    if ( itemsToSkip[0] > 0 )
                                                    {
                                                        itemsToSkip[0]--;
                                                        return;
                                                    }

                                                    itemsToSkip[0] = v.SkipItems;
                                                    observer.OnNext(v.V);
                                                });
                });
        }
    }
}
