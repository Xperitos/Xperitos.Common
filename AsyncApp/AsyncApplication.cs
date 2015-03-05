using System;
using System.Reactive.Concurrency;

namespace Xperitos.Common.AsyncApp
{
    /// <summary>
    /// Derive to implement a message loop based application. Call "Run" from Main()
    /// </summary>
    public abstract class AsyncApplication : MessageLoop
    {
        protected AsyncApplication()
        {
            if (Instance != null)
                throw new InvalidOperationException("An app object already exists!");

            Instance = this;
        }

        /// <summary>
        /// Access the singleton app.
        /// </summary>
        public static AsyncApplication Instance { get; private set; }
    }
}
