using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using ReactiveUI;

namespace Xperitos.Common.Utils
{
    /// <summary>
    /// Base class for async objects using the dispose pattern.
    /// </summary>
    public abstract class DisposableAsyncObject : ReactiveObject, IDisposable
    {
        protected DisposableAsyncObject(IScheduler scheduler)
        {
            Scheduler = scheduler;
            Disposables = new CompositeDisposable();

            m_tokenSource = new CancellationDisposable();
            DisposedToken = m_tokenSource.Token;
        }

        /// <summary>
        /// The scheduler associated with this async object.
        /// </summary>
        public IScheduler Scheduler { get; private set; }

        /// <summary>
        /// Use this in derived classes to register disposable objects.
        /// </summary>
        protected CompositeDisposable Disposables { get; private set; }

        /// <summary>
        /// Watch this for service disposal event.
        /// </summary>
        protected CancellationToken DisposedToken { get; private set; }

        private readonly CancellationDisposable m_tokenSource;

        public void Dispose()
        {
            if (Disposables.IsDisposed)
                return;

            m_tokenSource.Dispose();

            Disposables.Dispose();
        }
    }
}
