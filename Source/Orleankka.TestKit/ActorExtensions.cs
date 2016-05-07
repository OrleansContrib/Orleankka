namespace Orleankka.TestKit
{
    using Core;
    using Utility;

    public static class ActorExtensions
    {
        public static TActor Define<TActor>(this TActor actor) where TActor : Actor
        {
            Requires.NotNull(actor, nameof(actor));
            actor.Type = ActorType.From(actor.GetType());
            return actor;
        }
    }
}
