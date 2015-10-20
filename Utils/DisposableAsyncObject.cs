using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Xperitos.Common.Utils
{
    public class DisposableAsyncObject : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
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

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        internal void RaisePropertyChanging(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        internal void RaisePropertyChanged(string propertyName)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }
    }
}
