using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Xperitos.Common.DiApp
{
	/// <summary>
	/// Startup interface for DI console apps.
	/// </summary>
	public interface IStartupBase
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

	public interface IStartupAsync : IStartupBase
	{
		/// <summary>
		/// Executes the application - program terminates when task completes.
		/// </summary>
		/// <param name="services"></param>
		Task<int> RunAsync(IServiceProvider services);
	}

	public interface IStartup : IStartupBase
	{
		/// <summary>
		/// Executes the application 
		/// </summary>
		/// <param name="services">Injected services</param>
		/// <param name="terminateFunc">Call the function to terminate the program</param>
		void Run(IServiceProvider services, Func<int, Task> terminateFunc);
	}
}