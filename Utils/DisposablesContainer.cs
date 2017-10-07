using System;
using System.Reactive.Disposables;

namespace Xperitos.Common.Utils
{
	/// <summary>
	/// Holds a collection of disposable objects disposed upon exit (<see cref="DisposableMixins.ComposeDispose{T}(T,IDisposablesContainer)"/>
	/// </summary>
	public interface IDisposablesContainer
	{
		/// <summary>
		/// Holds list of disposables.
		/// </summary>
		CompositeDisposable Disposables { get; }
	}

	/// <summary>
	/// Simple implementation if <see cref="IDisposablesContainer"/>
	/// </summary>
	public sealed class DisposablesContainer : IDisposablesContainer, ICancelable
	{
		public CompositeDisposable Disposables { get; } = new CompositeDisposable();

		public void Dispose()
		{
			Disposables.Dispose();
		}

		public bool IsDisposed => Disposables.IsDisposed;
	}
}