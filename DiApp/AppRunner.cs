using System;
using System.Reactive.Concurrency;
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
	public sealed class AppRunner<T> : AsyncApplication where T : IStartupBase, new()
	{
		private readonly T m_startup;
		private IServiceProvider m_serviceProvider;
		private string[] m_args;

		private int m_exitCode = 0;

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
			serviceCollection.AddSingleton(Log.Logger);
			serviceCollection.AddSingleton(scheduler);

			m_startup.ConfigureServices(serviceCollection);

			m_serviceProvider = serviceCollection.BuildServiceProvider();

			if (m_serviceProvider is IDisposable)
				Disposables.Add((IDisposable)m_serviceProvider);

			scheduler.Schedule(RunInternal);

			return true;
		}

		private void RunInternal()
		{
			Log.Debug("Running");

			switch (m_startup)
			{
				case IStartupAsync asyncStartup:
					asyncStartup.RunAsync(m_serviceProvider).ContinueWith(t =>
					{
						m_exitCode = t.Result;
						return QuitAsync();
					});
					break;
				case IStartup syncStartup:
					syncStartup.Run(m_serviceProvider, (exitCode) =>
					{
						m_exitCode = exitCode;
						return QuitAsync();
					});
					break;
			}
		}
	}
}
