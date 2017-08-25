using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Xperitos.Common.Utils
{
	public interface IDisposablesContainer
	{
		/// <summary>
		/// Holds a collection of disposable objects disposed upon exit (<see cref="DisposableMixins.ComposeDispose{T}"/>
		/// </summary>
		CompositeDisposable Disposables { get; }
	}

	/// <summary>
	/// Indicates that the object has a thread affinity.
	/// </summary>
	public interface ISchedulerProvider
	{
		IScheduler Scheduler { get; }
	}

	public class DisposableAsyncObject : INotifyPropertyChanged, INotifyPropertyChanging, IDisposablesContainer, ICancelable, ISchedulerProvider
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
        public IScheduler Scheduler { get; }

        /// <summary>
        /// Use this in derived classes to register disposable objects.
        /// </summary>
        protected CompositeDisposable Disposables { get; }

		/// <summary>
		/// True if the object was disposed.
		/// </summary>
		public bool IsDisposed => Disposables.IsDisposed;

		/// <summary>
		/// Watch this for service disposal event.
		/// </summary>
		protected CancellationToken DisposedToken { get; }

		CompositeDisposable IDisposablesContainer.Disposables => Disposables;

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

        private void RaisePropertyChanging(string propertyName)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected TRet RaiseAndSetIfChanged<TRet>(
            ref TRet backingField,
            TRet newValue,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TRet>.Default.Equals(backingField, newValue))
                return newValue;

            RaisePropertyChanging(propertyName);
            backingField = newValue;
            RaisePropertyChanged(propertyName);
            return newValue;
        }
	}
}
