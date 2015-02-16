using System;
using System.Linq;

namespace Orleankka
{
    class ActorConfiguration
    {
        public static ActorConfiguration Default = new ActorConfiguration(); 

        internal readonly Activation Activation = Activation.Actor;
        internal readonly Placement Placement = Placement.Random;
        internal readonly Concurrency Concurrency = Concurrency.Sequential;
        internal readonly Delivery Delivery = Delivery.Ordered;

        ActorConfiguration()
        {}

        ActorConfiguration(
            Activation activation,
            Placement placement,
            Concurrency concurrency,
            Delivery delivery)
        {
            Activation = activation;
            Placement = placement;
            Concurrency = concurrency;
            Delivery = delivery;
        }

        internal static ActorConfiguration Actor(Placement placement, Concurrency concurrency, Delivery delivery)
        {
            return new ActorConfiguration(Activation.Actor, placement, concurrency, delivery);
        }
        
        internal static ActorConfiguration Worker(Concurrency concurrency, Delivery delivery)
        {
            return new ActorConfiguration(Activation.Worker, Placement.Random, concurrency, delivery);
        }
    }

    public class ActorConfigurationAttribute : Attribute
    {
        internal ActorConfiguration Configuration;
    }
}
