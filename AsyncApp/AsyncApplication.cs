using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using Xperitos.Common.AsyncApp.Reactive;
using Xperitos.Common.Utils;

namespace Xperitos.Common.AsyncApp
{
    /// <summary>
    /// Derive to implement a message loop based application. Call "Run" from Main()
    /// </summary>
    public abstract class AsyncApplication : MessageLoop
    {
        protected AsyncApplication()
        {
            if (Instance != null)
                throw new InvalidOperationException("An app object already exists!");

            Instance = this;
            RxApp.MainThreadScheduler = new MessageLoopScheduler(this);

            Disposables = new CompositeDisposable();

            var cancellationToken = new CancellationDisposable();
            cancellationToken.ComposeDispose(Disposables);

            DisposedToken = cancellationToken.Token;
        }

        /// <summary>
        /// Access the singleton app.
        /// </summary>
        public static AsyncApplication Instance { get; private set; }

        /// <summary>
        /// Holds a collection of disposable objects disposed upon exit (<see cref="DisposableMixins.ComposeDispose{T}"/>
        /// </summary>
        protected CompositeDisposable Disposables { get; private set; }

        /// <summary>
        /// Token is cancelled upon exit.
        /// </summary>
        protected CancellationToken DisposedToken { get; private set; }

        protected override async Task OnExitAsync()
        {
            Disposables.Dispose();
            await base.OnExitAsync();
        }
    }
}
