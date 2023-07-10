using System.Diagnostics;
using System.Threading.Tasks;

using Orleankka.Utility;

namespace Orleankka
{
    public static class StreamRefServerExtensions
    {
        public static Task Subscribe<TItem>(this StreamRef<TItem> stream, ActorGrain actor) =>
            stream.Subscribe(actor, new SubscribeReceiveItem());

        public static async Task Subscribe<TItem, TOptions>(this StreamRef<TItem> stream, ActorGrain actor, TOptions options) 
            where TOptions : SubscribeOptions
        {
            Requires.NotNull(actor, nameof(actor));

            var subscriptions = await stream.Subscriptions();
            if (subscriptions.Count == 1)
                return;

            Debug.Assert(subscriptions.Count == 0,
                "We should keep only one active subscription per-stream per-actor");

            await stream.Subscribe(actor.ReceiveRequest, options);
        }

        public static async Task Unsubscribe<TItem>(this StreamRef<TItem> stream, ActorGrain actor)
        {
            Requires.NotNull(actor, nameof(actor));

            var subscriptions = await stream.Subscriptions();
            if (subscriptions.Count == 0)
                return;

            Debug.Assert(subscriptions.Count == 1,
                "We should keep only one active subscription per-stream per-actor");

            await subscriptions[0].Unsubscribe();
        }

        public static Task Resume<TItem>(this StreamRef<TItem> stream, ActorGrain actor) =>
            stream.Resume(actor, new ResumeReceiveItem());

        public static async Task Resume<TItem, TOptions>(this StreamRef<TItem> stream, ActorGrain actor, TOptions options)
            where TOptions : ResumeOptions
        {
            Requires.NotNull(actor, nameof(actor));

            var subscriptions = await stream.Subscriptions();
            if (subscriptions.Count == 0)
                return;

            Debug.Assert(subscriptions.Count == 1,
                "We should keep only one active subscription per-stream per-actor");

            await subscriptions[0].Resume(actor.ReceiveRequest, options);
        }
    }
}