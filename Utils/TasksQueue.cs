using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    /// <summary>
    /// A queue that performs the enqueued task one after another.
    /// </summary>
    public class TasksQueue : IDisposable
    {
        public TasksQueue(SynchronizationContext ctx = null)
        {
            m_ctx = ctx ?? SynchronizationContext.Current;

            m_ctx.Post(async () => await PumpTasksAsync());
        }

        private async Task PumpTasksAsync()
        {
            while (!m_disposable.IsDisposed)
            {
                try
                {
                    // Yield to the scheduler.
                    await Task.Yield();

                    // Wait for item to become available.
                    await m_countSemaphore.WaitAsync(m_disposable.Token);

                    using (await m_taskRunningSemaphore.LockAsync(m_disposable.Token))
                    {
                        Func<CancellationToken, Task> item;
                        lock (m_pendingTasks)
                            item = m_pendingTasks.Dequeue();

                        // Perform the action.
                        await item(m_disposable.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Catch cancelled exception (but only if it's ours).
                    if (!m_disposable.IsDisposed)
                        throw;
                }
            }
        }

        /// <summary>
        /// Schedule a task.
        /// </summary>
        /// <param name="task">Task to run</param>
        public void Enqueue(Func<Task> task)
        {
            Enqueue((ct) => task());
        }

        /// <summary>
        /// Schedule a task.
        /// </summary>
        /// <param name="task">Task to run</param>
        public void Enqueue(Func<CancellationToken, Task> task)
        {
            ThrowIfDisposedOrCompleted();
            EnqueueInternal(task);
        }
        private void EnqueueInternal(Func<CancellationToken, Task> task)
        {
            lock (m_pendingTasks)
            {
                m_pendingTasks.Enqueue(task);
                m_countSemaphore.Release();
            }
        }

        private readonly SynchronizationContext m_ctx;

        private readonly Queue<Func<CancellationToken, Task>> m_pendingTasks = new Queue<Func<CancellationToken, Task>>();
        private readonly SemaphoreSlim m_countSemaphore = new SemaphoreSlim(0);
        private readonly SemaphoreSlim m_taskRunningSemaphore = new SemaphoreSlim(1);

        private Func<CancellationToken, Task> m_completionTask = null;

        private void ThrowIfDisposedOrCompleted()
        {
            if (m_completionTask != null || m_disposable.IsDisposed)
                throw new InvalidOperationException("Queue disposed or completed");
        }

        /// <summary>
        /// Returns a task that is signaled when all the tasks upto this point are complete.
        /// </summary>
        public Task Checkpoint()
        {
            var cts = new TaskCompletionSource<bool>();

            // Add a task to be executed and signal the caller.
            Func<CancellationToken, Task> task = ct =>
            {
                // Mark completion to the awaiter.
                cts.SetResult(true);

                // Finish this task.
                return Task.FromResult(true);
            };

            Enqueue(task);

            return cts.Task;
        }

        /// <summary>
        /// Returns a task that is signaled when the queue is complete.
        /// </summary>
        public Task CompleteAsync()
        {
            var cts = new TaskCompletionSource<bool>();

            // Add a task to be executed and signal the caller.
            Func<CancellationToken, Task> completionTask = ct =>
            {
                // Mark completion to the awaiter.
                cts.SetResult(true);

                // Terminate the message pump.
                m_ctx.Post(Dispose);

                // Finish this task.
                return Task.FromResult(true);
            };

            if (Interlocked.CompareExchange(ref m_completionTask, completionTask, null) != null)
                throw new InvalidOperationException("Already completed!");

            // Enqueue for execution.
            EnqueueInternal(m_completionTask);

            return cts.Task;
        }

        private readonly CancellationDisposable m_disposable = new CancellationDisposable();

        public void Dispose()
        {
            m_disposable.Dispose();
        }
    }
}
