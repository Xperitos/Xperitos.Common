using System;
using System.Threading.Tasks;

namespace Xperitos.Common.AsyncApp.Impl
{
    /// <summary>
    /// Implement a messge loop with init/terminate callbacks.
    /// </summary>
    class MessageLoopApp : MessageLoop
    {
        public void SetOnStart(Func<bool> onStart)
        {
            m_onStart = onStart;
        }

        public void SetOnExitAsync(Func<Task> onExit)
        {
            m_onExitAsync = onExit;
        }

        private Func<bool> m_onStart;
        private Func<Task> m_onExitAsync;

        #region Overrides of AsyncApplication

        protected override bool OnInit()
        {
            if (m_onStart == null)
                return true;

            return m_onStart();
        }

        protected override Task OnExitAsync()
        {
            if (m_onExitAsync != null)
                return m_onExitAsync();

            return Task.FromResult(true);
        }

        #endregion
    }
}