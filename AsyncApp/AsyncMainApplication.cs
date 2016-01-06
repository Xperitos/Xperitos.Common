using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xperitos.Common.Utils;

namespace Xperitos.Common.AsyncApp
{
    /// <summary>
    /// Implements an async app with a "Main" entry point.
    /// </summary>
    public abstract class AsyncMainApplication : AsyncApplication
    {
        protected sealed override bool OnInit()
        {
            this.Post(async () =>
            {
                // Wait for main to terminate.
                await MainAsync();

                // Close the application.
                Dispose();
            });

            return true;
        }

        /// <summary>
        /// Seal the exit function.
        /// </summary>
        /// <returns></returns>
        protected sealed override Task OnExitAsync()
        {
            return base.OnExitAsync();
        }

        /// <summary>
        /// Override to implement application logic.
        /// </summary>
        /// <returns></returns>
        protected abstract Task MainAsync();
    }
}
