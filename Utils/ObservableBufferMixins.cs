using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    static public class ObservableBufferMixins
    {
        /// <summary>
        /// Overload the buffer function that accepts a closing selector - it receives the current buffer and return true if it should close.
        /// </summary>
        static public IObservable<IList<T>> Buffer<T>(this IObservable<T> observable, Func<IList<T>, bool> closingSelector)
        {
            return Observable.Create<IList<T>>(
                observer =>
                {
                    var currentBuffer = new List<T>();

                    return observable.Subscribe((v) =>
                    {
                        currentBuffer.Add(v);
                        if (closingSelector(currentBuffer))
                        {
                            var buffer = currentBuffer;
                            currentBuffer = new List<T>();

                            // Pass it to the observer.
                            observer.OnNext(buffer);
                        }
                    }, observer.OnError, observer.OnCompleted);
                });
        }
    }
}
