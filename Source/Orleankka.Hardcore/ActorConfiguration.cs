using System;
using System.Linq;

namespace Orleankka
{
    class ActorConfiguration
    {
        public static readonly ActorConfiguration Default = new ActorConfiguration(); 

        internal readonly Activation Activation = Activation.Actor;
        internal readonly Placement Placement = Placement.Auto;
        internal readonly Delivery Delivery = Delivery.Ordered;

        ActorConfiguration()
        {}

        ActorConfiguration(
            Activation activation,
            Placement placement,
            Delivery delivery)
        {
            Activation = activation;
            Placement = placement;
            Delivery = delivery;
        }

        internal static ActorConfiguration Actor(Placement placement, Delivery delivery)
        {
            return new ActorConfiguration(Activation.Actor, placement, delivery);
        }
        
        internal static ActorConfiguration Worker(Delivery delivery)
        {
            return new ActorConfiguration(Activation.Worker, Placement.Auto, delivery);
        }
    }

    public class ActorConfigurationAttribute : Attribute
    {
        internal ActorConfiguration Configuration;
    }
}
