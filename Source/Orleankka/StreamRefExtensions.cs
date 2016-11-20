using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Orleankka.Utility;

namespace Orleankka
{
    public static class StreamRefExtensions
    {
        public static async Task Subscribe(this StreamRef stream, Actor actor, StreamFilter filter = null)
        {
            Requires.NotNull(actor, nameof(actor));

            var subscriptions = await stream.Subscriptions();
            if (subscriptions.Count == 1)
                return;

            Debug.Assert(subscriptions.Count == 0,
                "We should keep only one active subscription per-stream per-actor");

            await stream.Subscribe(actor.OnReceive, filter ?? DeclaredHandlerOnlyFilter(actor));
        }

        public static async Task Unsubscribe(this StreamRef stream, Actor actor)
        {
            Requires.NotNull(actor, nameof(actor));

            var subscriptions = await stream.Subscriptions();
            if (subscriptions.Count == 0)
                return;

            Debug.Assert(subscriptions.Count == 1,
                "We should keep only one active subscription per-stream per-actor");

            await subscriptions[0].Unsubscribe();
        }

        public static async Task Resume(this StreamRef stream, Actor actor)
        {
            Requires.NotNull(actor, nameof(actor));

            var subscriptions = await stream.Subscriptions();
            if (subscriptions.Count == 0)
                return;

            Debug.Assert(subscriptions.Count == 1,
                "We should keep only one active subscription per-stream per-actor");

            await subscriptions[0].Resume(actor.OnReceive);
        }

        static StreamFilter DeclaredHandlerOnlyFilter(Actor actor) => 
           new StreamFilter(actor.Dispatcher.Handlers);
    }
}