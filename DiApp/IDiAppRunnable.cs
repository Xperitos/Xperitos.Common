using System;

namespace Xperitos.Common.DiApp
{
	/// <summary>
	/// When registered in the DI, the "run" function will be called for each singleton after services are ready.
	/// </summary>
	public interface IDiAppRunnable
	{
		void Run();
	}

}