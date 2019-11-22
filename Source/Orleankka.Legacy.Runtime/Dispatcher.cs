using System;
using System.Threading.Tasks;

namespace Orleankka.Legacy
{
    public static class LegacyDispatcherExtensions
    {
        public static Task<object> Dispatch(this Dispatcher dispatcher, object target, object message, Func<object, Task<object>> fallback = null) => 
            dispatcher.DispatchResultAsync(target, message, fallback);

        public static void DispatchAction(this Dispatcher dispatcher, object target, object message, Func<object, Task<object>> fallback = null) =>
            dispatcher.Dispatch(target, message, fallback);

        public static object DispatchFunc(this Dispatcher dispatcher, object target, object message, Func<object, Task<object>> fallback = null) =>
            dispatcher.DispatchResult(target, message, fallback);

    }
}