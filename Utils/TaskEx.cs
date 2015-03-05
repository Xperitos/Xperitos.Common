using System.Threading.Tasks;

namespace Xperitos.Common.Utils
{
    public static class TaskEx
    {
        private static readonly Task m_completed = Task.FromResult(true);

        public static Task CompletedTask { get { return m_completed; } }
    }
}
