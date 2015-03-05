using System.Threading;

namespace Xperitos.Common.AsyncApp
{
    public interface ISyncContextProvider
    {
        SynchronizationContext SyncContext { get; }
    }
}
