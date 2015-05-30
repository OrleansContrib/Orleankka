using System;
using System.Linq;

namespace Orleankka
{
    public abstract class UntypedActor : Actor<UntypedActorPrototype>
    {
        protected UntypedActor()
        {}

        protected UntypedActor(string id, IActorSystem system)
            : base(id, system)
        {}
    }

    public class UntypedActorPrototype : ActorPrototype
    {
        public UntypedActorPrototype(Type actor)
            : base(actor)
        {}
    }
}