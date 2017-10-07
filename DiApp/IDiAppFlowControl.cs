using System;
using System.Threading.Tasks;

namespace Xperitos.Common.DiApp
{
	/// <summary>
	/// Control the flow of the DiApp (allows for termination).
	/// </summary>
	public interface IDiAppFlowControl
	{
		/// <summary>
		/// Returns the list of startup arguments.
		/// </summary>
		string[] StartupArguments { get; }

		/// <summary>
		/// Register a handler to be called when terminate is requested.
		/// </summary>
		/// <returns>Disposable - dispose to unregister the handler</returns>
		IDisposable RegisterTerminateHandler(Func<Task> asyncHandler);

		/// <summary>
		/// Request to termiante the DiApp.
		/// </summary>
		Task TerminateAsync(int exitCode = 0);
	}
}