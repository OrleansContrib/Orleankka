using System;
using System.Threading.Tasks;

namespace Orleankka.Extensions
{
    using Services;

    public static class TimerServiceExtensions
    {
        public static void Register(this ITimerService timers, TimeSpan due, TimeSpan period, Func<Task> callback) =>
            timers.Register(callback.Method.Name, due, period, callback);

        public static void Unregister(this ITimerService timers, Func<Task> callback) =>
            timers.Unregister(callback.Method.Name);

        public static bool IsRegistered(this ITimerService timers, Func<Task> callback) =>
            timers.IsRegistered(callback.Method.Name);
    }
}