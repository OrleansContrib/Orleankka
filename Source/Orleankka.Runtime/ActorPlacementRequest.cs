using System;

using Orleans.Runtime.Placement;

namespace Orleankka
{
    using Core;

    public struct ActorPlacementRequest
    {
        public static ActorPlacementRequest From(PlacementTarget target)
        {
            var @interface = ActorType.Of(target.GrainIdentity.TypeCode).Interface;

            var id = target.GrainIdentity.PrimaryKeyString;
            var path = ActorPath.For(@interface.Mapping.CustomInterface, id);

            return new ActorPlacementRequest(path, @interface.Mapping);
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