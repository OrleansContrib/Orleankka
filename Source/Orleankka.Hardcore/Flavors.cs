using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans.Concurrency;
using Orleans.Placement;

namespace Orleankka.Core.Hardcore
{
    public class Flavor
    {
        public readonly Attribute Attribute;
        public readonly string AttributeText;
        public readonly string Name;

        protected Flavor(string name, Attribute attribute = null)
        {
            Attribute = attribute;
            AttributeText = attribute != null ? attribute.GetType().Name.Replace("Attribute", "") : null;
            Name = name ?? AttributeText;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Has<TAttribute>()
        {
            if (Attribute == null)
                return false;

            return Attribute is TAttribute;
        }
    }

    public class Category : IEnumerable<Flavor>
    {
        readonly List<Flavor> flavors;

        public Category(IEnumerable<Flavor> flavors)
        {
            this.flavors = flavors.ToList();
        }

        public static Category Of<T>() where T : Flavor
        {
            var flavors = typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(info => info.GetValue(null))
                .Cast<T>();

            return new Category(flavors);
        }

        public IEnumerator<Flavor> GetEnumerator()
        {
            return flavors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public abstract class TypeLevelFlavor : Flavor
    {
        protected TypeLevelFlavor(string name = null, Attribute attribute = null)
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

    public abstract class MethodLevelFlavor : Flavor
    {
        readonly Method method;

        protected MethodLevelFlavor(Method method, string name, Attribute attribute = null)
            : base(name, attribute)
        {
            this.method = method;
        }

        public bool AppliesToTell()
        {
            return method == Method.Tell || method == Method.Both;
        }

        public bool AppliesToAsk()
        {
            return method == Method.Ask || method == Method.Both;
        }

        protected enum Method
        {
            None,
            Both,
            Tell,
            Ask
        }
    }

    public class Activation : TypeLevelFlavor
    {
        public static readonly Activation Singleton = new Activation(name: "Singleton");
        public static readonly Activation StatelessWorker = new Activation(new StatelessWorkerAttribute());

        Activation(Attribute attribute = null, string name = null)
            : base(name, attribute)
        {}

        public static Activation Of(ActorConfiguration cfg)
        {
            switch (cfg.Activation)
            {
                case ActivationKind.Singleton:
                    return Singleton;
                case ActivationKind.StatelessWorker:
                    return StatelessWorker;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class Placement : TypeLevelFlavor
    {
        public static readonly Placement Default = new Placement(name: "DefaultPlacement");
        public static readonly Placement Random = new Placement(new RandomPlacementAttribute());
        public static readonly Placement PreferLocal = new Placement(new PreferLocalPlacementAttribute());
        public static readonly Placement DistributeEvenly = new Placement(new ActivationCountBasedPlacementAttribute());

        Placement(Attribute attribute = null, string name = null)
            : base(name, attribute)
        {}

        public static Placement Of(ActorConfiguration cfg)
        {
            switch (cfg.Placement)
            {
                case PlacementKind.Default:
                    return Default;                
                case PlacementKind.Random:
                    return Random;
                case PlacementKind.PreferLocal:
                    return PreferLocal;
                case PlacementKind.DistributeEvenly:
                    return DistributeEvenly;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class Concurrency : MethodLevelFlavor
    {
        public static readonly Concurrency Sequential = new Concurrency(Method.None, "Sequential");
        public static readonly Concurrency Reentrant = new Concurrency(Method.Both, "Reentrant", new AlwaysInterleaveAttribute());
        public static readonly Concurrency TellInterleave = new Concurrency(Method.Tell, "TellInterleave", new AlwaysInterleaveAttribute());
        public static readonly Concurrency AskInterleave = new Concurrency(Method.Ask, "AskInterleave", new AlwaysInterleaveAttribute());

        Concurrency(Method method, string name, Attribute attribute = null)
            : base(method, "Concurrency" + name, attribute)
        {}

        public static Concurrency Of(ActorConfiguration cfg)
        {
            switch (cfg.Concurrency)
            {
                case ConcurrencyKind.Sequential:
                    return Sequential;
                case ConcurrencyKind.Reentrant:
                    return Reentrant;
                case ConcurrencyKind.TellInterleave:
                    return TellInterleave;
                case ConcurrencyKind.AskInterleave:
                    return AskInterleave;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class Delivery : TypeLevelFlavor
    {
        public static readonly Delivery Ordered   = new Delivery("OrderedDelivery");
        public static readonly Delivery Unordered = new Delivery("UnorderedDelivery", new UnorderedAttribute());

        Delivery(string name, Attribute attribute = null)
            : base(name: name, attribute: attribute)
        {}

        protected override bool InterfaceTarget
        {
            get { return true; }
        }

        public static Delivery Of(ActorConfiguration cfg)
        {
            switch (cfg.Delivery)
            {
                case DeliveryKind.Ordered:
                    return Ordered;
                case DeliveryKind.Unordered:
                    return Unordered;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
