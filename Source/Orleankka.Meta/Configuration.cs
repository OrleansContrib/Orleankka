using System;
using System.Linq;

namespace Orleankka
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ActorConfigurationAttribute : Attribute
    {
        public readonly ActorConfiguration Configuration;

        public ActorConfigurationAttribute(
            ActivationKind activation = ActivationKind.Singleton, 
            PlacementKind placement = PlacementKind.Default, 
            ConcurrencyKind concurrency = ConcurrencyKind.Sequential, 
            DeliveryKind delivery = DeliveryKind.Ordered)
        {
            Configuration = new ActorConfiguration(activation, placement, concurrency, delivery);
        }
    }

    public class ActorConfiguration
    {
        public readonly ActivationKind Activation;
        public readonly PlacementKind Placement;
        public readonly ConcurrencyKind Concurrency;
        public readonly DeliveryKind Delivery;

        public ActorConfiguration(
            ActivationKind activation = ActivationKind.Singleton, 
            PlacementKind placement = PlacementKind.Default, 
            ConcurrencyKind concurrency = ConcurrencyKind.Sequential, 
            DeliveryKind delivery = DeliveryKind.Ordered)
        {
            Activation = activation;
            Placement = placement;
            Delivery = delivery;
            Concurrency = concurrency;
        }
    }
}
