using System.Threading.Tasks;

namespace Orleankka
{
    public static class TaskResult
    {
        public static Task<object> From(object x) => Task.FromResult<object>(x);
        public static readonly Task<object> Done = Task.FromResult<object>(Orleankka.Done.Result);
        public static readonly Task<object> Unhandled = Task.FromResult<object>(Orleankka.Unhandled.Result);
    }
}