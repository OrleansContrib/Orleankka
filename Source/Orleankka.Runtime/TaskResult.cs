using System.Threading.Tasks;

namespace Orleankka
{
    public static class TaskResult
    {
        public static readonly Task<object> Done = Task.FromResult<object>(Orleankka.Done.Result);
        public static readonly Task<object> Unhandled = Task.FromResult<object>(Orleankka.Unhandled.Result);
    }
}