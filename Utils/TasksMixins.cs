using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
	public static class TasksMixins
	{
		/// <summary>
		/// Use the task without doing anything with it. Useful for calling async tasks from sync code (and suppress the CS4014 warning).
		/// </summary>
		public static async void Forget(this Task task)
		{
			await task.ConfigureAwait(false);
		}

		/// <summary>
		/// Use the task without doing anything with it. Useful for calling async tasks from sync code (and suppress the CS4014 warning).
		/// </summary>
		public static async void ForgetLogExceptions(this Task task, Action<Exception> exceptionLogger = null)
		{
			try
			{
				await task.ConfigureAwait(false);
			}
			catch (Exception e)
			{
				exceptionLogger?.Invoke(e);
			}
		}
	}
}
