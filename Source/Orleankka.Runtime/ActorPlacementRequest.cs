using System;

using Orleans.Runtime.Placement;

namespace Orleankka
{
    using Core;

    public struct ActorPlacementRequest
    {
        public static ActorPlacementRequest From(PlacementTarget target)
        {
            var @interface = ActorInterface.Of(target.InterfaceId);
            var mapping = @interface.Mapping;

            var id = target.GrainIdentity.PrimaryKeyString;
            var path = new ActorPath(@interface.Name, id);

            return new ActorPlacementRequest(path, mapping);
        }

        public readonly ActorPath Path;
        public readonly Type CustomInterface;
        public readonly Type ImplementationClass;

        ActorPlacementRequest(ActorPath path, ActorInterfaceMapping mapping)
        {
            Path = path;
            CustomInterface = mapping.CustomInterface;
            ImplementationClass = mapping.ImplementationClass;
        }
    }
}