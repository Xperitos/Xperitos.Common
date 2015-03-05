using System.Reactive.Concurrency;
using System.Threading;

namespace Xperitos.Common.AsyncApp
{
    public interface ISyncContextProvider
    {
        SynchronizationContext SyncContext { get; }
    }
}
