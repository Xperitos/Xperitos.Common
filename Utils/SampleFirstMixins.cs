using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Xperitos.Common.Utils
{
    public static class SampleFirstMixins
    {
        /// <summary>
        /// Like <see cref="Observable.Sample{TSource}(System.IObservable{TSource},System.TimeSpan)"/> except that it produces the FIRST element and not the last
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IObservable<T> SampleFirst<T>(this IObservable<T> sequence, TimeSpan interval)
        {
            var scheduler = DefaultScheduler.Instance;

            return Observable.Create<T>(
                observer =>
                {
                    var lastSampleTime = DateTimeOffset.MinValue;

                    return sequence.Subscribe(v =>
                    {
                        var now = scheduler.Now;
                        if (now - lastSampleTime <= interval) 
                            return;

                        // Pass the sample and reset the timer.
                        lastSampleTime = now;
                        observer.OnNext(v);

                    }, observer.OnError, observer.OnCompleted);
                });
        }
    }
}