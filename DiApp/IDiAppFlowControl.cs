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
		/// Request to termiante the DiApp.
		/// </summary>
		Task TerminateAsync(int exitCode = 0);
	}
}