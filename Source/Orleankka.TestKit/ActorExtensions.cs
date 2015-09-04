using System;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    using Core;
    using Utility;

    public static class ActorExtensions
    {
        public static Task OnActivate(this Actor actor)
        {
            Requires.NotNull(actor, nameof(actor));
            return actor.OnActivate();
        }

        public static Task OnDeactivate(this Actor actor)
        {
            Requires.NotNull(actor, nameof(actor));
            return actor.OnDeactivate();
        }

        public static Task<object> OnReceive(this Actor actor, object message)
        {
            Requires.NotNull(actor, nameof(actor));
            return actor.OnReceive(message);
        }

        public static Task OnReminder(this Actor actor, string id)
        {
            Requires.NotNull(actor, nameof(actor));
            return actor.OnReminder(id);
        }

        public static void Define(this Actor actor)
        {
            Requires.NotNull(actor, nameof(actor));
            actor.Prototype = ActorPrototype.Define(actor.GetType());
        }

        public static void Dispatch(this Actor actor, object message, Action<object> fallback)
        {
            Requires.NotNull(actor, nameof(actor));
            actor.Prototype.Dispatch(actor, message, fallback);
        }

        public static TResult DispatchResult<TResult>(this Actor actor, object message, Func<object, object> fallback)
        {
            Requires.NotNull(actor, nameof(actor));
            return (TResult) DispatchResult(actor, message, fallback);
        }

        public static object DispatchResult(this Actor actor, object message, Func<object, object> fallback)
        {
            Requires.NotNull(actor, nameof(actor));
            return actor.Prototype.DispatchResult(actor, message, fallback);
        }

        public static async Task<TResult> DispatchAsync<TResult>(this Actor actor, object message, Func<object, Task<object>> fallback)
        {
            Requires.NotNull(actor, nameof(actor));
            return (TResult)(await DispatchAsync(actor, message, fallback));
        }

        public static Task<object> DispatchAsync(this Actor actor, object message, Func<object, Task<object>> fallback)
        {
            Requires.NotNull(actor, nameof(actor));
            return actor.Prototype.DispatchAsync(actor, message, fallback);
        }
    }
}
