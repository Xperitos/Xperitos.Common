using System;
using System.Reactive.Disposables;

namespace Xperitos.Common.Utils
{
    public static class DisposableMixins
    {
        /// <summary>
        /// Linq friendly function to add a disposable to a composite disposable.
        /// </summary>
        public static T ComposeDispose<T>(this T disposable, CompositeDisposable compositeDisposable)
            where T : IDisposable
        {
            compositeDisposable.Add(disposable);
            return disposable;
        }
    }
}
