using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Literate;
using Xperitos.Common.AsyncApp;
using Xperitos.Common.Utils;

namespace Xperitos.Common.DiApp
{
	/// <summary>
	/// Root for dependency injection async app.
	/// </summary>
	/// <example>
	/// 	class ProgramMain
	/// 	{
	/// 		static void Main(string[] args)
	/// 		{
	/// 			var pgm = new ApplicationRunner&lt;Startup&gt;();
	/// 			pgm.Run(args);
	/// 		}
	/// 	}
	/// </example>
	/// <typeparam name="T"></typeparam>
	public sealed class AppRunner<T> : AsyncApplication, IDiAppFlowControl
		where T : IDiAppStartup, new()
	{
		private readonly T m_startup;
		private IServiceProvider m_serviceProvider;
		private string[] m_args;

		private int m_exitCode;

		public AppRunner()
		{
			m_startup = new T();
			m_args = new string[0];
		}

		/// <summary>
		/// Launch with arguments.
		/// </summary>
		public int Run(string[] args)
		{
			m_args = args;
			Run();

			return m_exitCode;
		}

		protected override bool OnInit()
		{
			Log.Debug("Initializing");

			if (!m_startup.Init(m_args ?? new string[0]))
				return false;

			var scheduler = this.GetScheduler();

			var serviceCollection = new ServiceCollection();

			//
			// Add internal service
			//
			serviceCollection.AddSingleton(Log.Logger);
			serviceCollection.AddSingleton(scheduler);
			serviceCollection.AddSingleton<IDiAppFlowControl>(this);

			// Allow the app to register services.
			m_startup.ConfigureServices(serviceCollection);

			m_serviceProvider = serviceCollection.BuildServiceProvider();

			scheduler.Schedule(RunInternal);

			return true;
		}

		private void RunInternal()
		{
			Log.Debug("Running");

			// Start all the runnables.
			var runnables = m_serviceProvider.GetServices<IDiAppRunnable>();
			foreach (var r in runnables)
				r.Run();
		}

		public string[] StartupArguments => m_args;

		private readonly List<Func<Task>> m_terminateHandlers = new List<Func<Task>>();

		public IDisposable RegisterTerminateHandler(Func<Task> asyncHandler)
		{
			m_terminateHandlers.Add(asyncHandler);
			return Disposable.Create(() => m_terminateHandlers.Remove(asyncHandler));
		}

		public async Task TerminateAsync(int exitCode = 0)
		{
			m_exitCode = exitCode;

			// Call each handler.
			foreach (var h in m_terminateHandlers.ToList())
				await h();

			// Request to quit.
			await QuitAsync();
		}
	}
}
