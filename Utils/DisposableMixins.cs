using System;
using System.Reactive.Disposables;

namespace Xperitos.Common.Utils
{
    public static class DisposableMixins
    {
        /// <summary>
        /// Linq friendly function to add a disposable to a composite disposable.
        /// </summary>
        public static void ComposeDispose(this IDisposable disposable, CompositeDisposable compositeDisposable)
        {
            compositeDisposable.Add(disposable);
        }
    }
}
