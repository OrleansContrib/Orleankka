using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans.Concurrency;
using Orleans.Placement;

namespace Orleankka.Codegen
{
    public class ActorEndpointAttribute
    {
        public readonly Attribute Attribute;
        public readonly string AttributeText;
        public readonly string Name;

        protected ActorEndpointAttribute(string name, Attribute attribute = null)
        {
            Name = name;
            Attribute = attribute;
            AttributeText = attribute != null ? attribute.GetType().Name.Replace("Attribute", "") : null;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class ActorEndpointAttributeCategory : IEnumerable<ActorEndpointAttribute>
    {
        readonly List<ActorEndpointAttribute> attributes;

        ActorEndpointAttributeCategory(IEnumerable<ActorEndpointAttribute> attributes)
        {
            this.attributes = attributes.ToList();
        }

        public static ActorEndpointAttributeCategory Of<T>() where T : ActorEndpointAttribute
        {
            var attributes = typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(info => info.GetValue(null))
                .Cast<T>();

            return new ActorEndpointAttributeCategory(attributes);
        }

        public IEnumerator<ActorEndpointAttribute> GetEnumerator()
        {
            return attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public abstract class TypeAttribute : ActorEndpointAttribute
    {
        protected TypeAttribute(string name, Attribute attribute = null)
            : base(name, attribute)
        {}

        public bool AppliesToInterface()
        {
            return Attribute != null && InterfaceTarget;
        }

        public bool AppliesToClass()
        {
            return Attribute != null && !InterfaceTarget;
        }

        protected virtual bool InterfaceTarget
        {
            get { return false; }
        }
    }

    public class ActivationAttribute : TypeAttribute
    {
        public static readonly ActivationAttribute Actor  = new ActivationAttribute("Actor");
        public static readonly ActivationAttribute Worker = new ActivationAttribute("Worker", new StatelessWorkerAttribute());

        ActivationAttribute(string name, Attribute attribute = null)
            : base(name, attribute)
        {}

        internal static ActivationAttribute Of(ActorConfiguration cfg)
        {
            switch (cfg.Activation)
            {
                case Activation.Actor:
                    return Actor;
                case Activation.Worker:
                    return Worker;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class PlacementAttribute : TypeAttribute
    {
        public static readonly PlacementAttribute Auto = new PlacementAttribute("AutoPlacement");
        public static readonly PlacementAttribute PreferLocal = new PlacementAttribute("PreferLocalPlacement", new PreferLocalPlacementAttribute());
        public static readonly PlacementAttribute DistributeEvenly = new PlacementAttribute("EvenDistributionPlacement", new ActivationCountBasedPlacementAttribute());

        PlacementAttribute(string name, Attribute attribute = null)
            : base(name, attribute)
        {}

        internal static PlacementAttribute Of(ActorConfiguration cfg)
        {
            switch (cfg.Placement)
            {
                case Placement.Auto:
                    return Auto;
                case Placement.PreferLocal:
                    return PreferLocal;
                case Placement.DistributeEvenly:
                    return DistributeEvenly;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class DeliveryAttribute : TypeAttribute
    {
        public static readonly DeliveryAttribute Ordered   = new DeliveryAttribute("OrderedDelivery");
        public static readonly DeliveryAttribute Unordered = new DeliveryAttribute("UnorderedDelivery", new UnorderedAttribute());

        DeliveryAttribute(string name, Attribute attribute = null)
            : base(name, attribute)
        {}

        protected override bool InterfaceTarget
        {
            get { return true; }
        }

        internal static DeliveryAttribute Of(ActorConfiguration cfg)
        {
            switch (cfg.Delivery)
            {
                case Delivery.Ordered:
                    return Ordered;
                case Delivery.Unordered:
                    return Unordered;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
