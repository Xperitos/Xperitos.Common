using Microsoft.Extensions.DependencyInjection;

namespace Xperitos.Common.DiApp
{
	/// <summary>
	/// Startup interface for DI console apps.
	/// </summary>
	public interface IDiAppStartup
	{
		/// <summary>
		/// Called upon construction to initialize the app using the given args
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		bool Init(string[] args);

		/// <summary>
		/// Called to configure the DI services.
		/// </summary>
		/// <param name="collection"></param>
		void ConfigureServices(IServiceCollection collection);
	}
}