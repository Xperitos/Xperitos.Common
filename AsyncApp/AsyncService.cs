using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.AsyncApp
{
    public class AsyncService : ServiceBase, ISyncContextProvider
    {
        protected AsyncService()
        {
        }

        /// <summary>
        /// Used internally for debug
        /// </summary>
        public void DoStart()
        {
            OnStart(new string[0]);
        }

        /// <summary>
        /// Used internally for debug
        /// </summary>
        public void DoStop()
        {
            OnStop();
        }

        private IDisposable m_terminateServiceDisposable;

        protected sealed override void OnStart(string[] args)
        {
            m_terminateServiceDisposable = MessageLoop.RunThread(OnInitializedInternal, OnTerminatedAsync);
        }

        protected sealed override void OnStop()
        {
            if ( m_terminateServiceDisposable == null )
                return;

            m_terminateServiceDisposable.Dispose();
            m_terminateServiceDisposable = null;
        }

        private void OnInitializedInternal()
        {
            // Initialize the sync context associated with this service.
            SyncContext = MessageLoop.SyncContext;

            OnInitialized();
        }

        public SynchronizationContext SyncContext { get; private set; }

        /// <summary>
        /// Called from the service thread - when stopped pumping messages.
        /// </summary>
        protected virtual Task OnTerminatedAsync()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Called from the service thread - start pumping messages.
        /// </summary>
        protected virtual void OnInitialized()
        {
            
        }
    }
}
