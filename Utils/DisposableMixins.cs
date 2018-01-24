using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

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

		/// <summary>
		/// Linq friendly function to add a disposable to a composite disposable.
		/// </summary>
		public static T ComposeDispose<T>(this T disposable, IDisposablesContainer compositeDisposable)
            where T : IDisposable
        {
            compositeDisposable.Disposables.Add(disposable);
            return disposable;
        }

		/// <summary>
		/// Take values until cancellation token is signaled.
		/// </summary>
	    public static IObservable<T> TakeUntil<T>(this IObservable<T> observable, CancellationToken ct)
	    {
		    return Observable.Create<T>(observer =>
		    {
			    CompositeDisposable disposables = new CompositeDisposable();

			    var subject = new Subject<Unit>();
			    var ctDisposable = ct.Register(() => subject.OnNext(Unit.Default));

				disposables.Add(ctDisposable);
				disposables.Add(observable.TakeUntil(subject).Subscribe(observer));

			    return disposables;
		    });
	    }
    }
}
