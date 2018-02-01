using System.Threading.Tasks;

namespace Orleankka
{
    public static class TaskResult
    {
        public static readonly Task<object> Done = Task.FromResult<object>(Orleankka.Done.Message);
        public static readonly Task<object> Unhandled = Task.FromResult<object>(Orleankka.Unhandled.Message);
    }
}