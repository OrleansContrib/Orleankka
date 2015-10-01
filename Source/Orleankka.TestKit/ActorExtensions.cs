namespace Orleankka.TestKit
{
    using Core;
    using Utility;

    public static class ActorExtensions
    {
        public static TActor Define<TActor>(this TActor actor) where TActor : Actor
        {
            Requires.NotNull(actor, nameof(actor));
            actor.Prototype = ActorPrototype.Define(actor.GetType());
            return actor;
        }
    }
}
