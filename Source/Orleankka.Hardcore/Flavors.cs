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

        protected Flavor(Attribute attribute = null, string name = null)
        {
            Attribute = attribute;
            AttributeText = attribute != null ? attribute.GetType().Name.Replace("Attribute", "") : null;
            Name = name ?? (attribute != null ? AttributeText : "Default");
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

        protected static bool Has<TAttribute>(MemberInfo info) where TAttribute : Attribute
        {
            return info.GetCustomAttributes<TAttribute>().Any();
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
        protected TypeLevelFlavor(Attribute attribute = null, string name = null)
            : base(attribute, name)
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

        protected MethodLevelFlavor(Method method, Attribute attribute = null, string name = null)
            : base(attribute, method != Method.None ? Enum.GetName(typeof(Method), method) + name : null)
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
            : base(attribute, name)
        {}

        public static Activation Of(Type type)
        {
            return Has<StatelessWorkerAttribute>(type) ? StatelessWorker : Singleton;
        }
    }

    public class Concurrency : TypeLevelFlavor
    {
        public static readonly Concurrency Default = new Concurrency();
        public static readonly Concurrency Reentrant = new Concurrency(new ReentrantAttribute());

        Concurrency(Attribute attribute = null, string name = null)
            : base(attribute, name)
        {}

        public static Concurrency Of(Type type)
        {
            return Has<ReentrantAttribute>(type) ? Reentrant : Default;
        }
    }

    public class Placement : TypeLevelFlavor
    {
        public static readonly Placement Default = new Placement();
        public static readonly Placement Random = new Placement(new RandomPlacementAttribute());
        public static readonly Placement PreferLocal = new Placement(new PreferLocalPlacementAttribute());
        public static readonly Placement ActivationCountBased = new Placement(new ActivationCountBasedPlacementAttribute());

        Placement(Attribute attribute = null, string name = null)
            : base(attribute, name)
        {}

        public static Placement Of(Type type)
        {
            if (Has<RandomPlacementAttribute>(type))
                return Random;
            
            if (Has<PreferLocalPlacementAttribute>(type))
                return PreferLocal;
            
            if (Has<ActivationCountBasedPlacementAttribute>(type))
                return ActivationCountBased;

            return Default;
        }
    }

    public class Delivery : TypeLevelFlavor
    {
        public static readonly Delivery Default = new Delivery();
        public static readonly Delivery Unordered = new Delivery(new UnorderedAttribute());

        Delivery(Attribute attribute = null, string name = null)
            : base(attribute, name)
        {}

        protected override bool InterfaceTarget
        {
            get { return true; }
        }

        public static Delivery Of(Type type)
        {
            return Has<UnorderedAttribute>(type) ? Unordered : Default;
        }
    }

    public class Interleave : MethodLevelFlavor
    {
        public static readonly Interleave Default = new Interleave(Method.None);
        public static readonly Interleave Both = new Interleave(Method.Both, new AlwaysInterleaveAttribute());
        public static readonly Interleave Tell = new Interleave(Method.Tell, new AlwaysInterleaveAttribute());
        public static readonly Interleave Ask = new Interleave(Method.Ask, new AlwaysInterleaveAttribute());

        Interleave(Method method, Attribute attribute = null)
            : base(method, attribute, "Interleave")
        {}

        public static Interleave Of(Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            var onTell = type.GetMethod("OnTell", flags);
            var onAsk = type.GetMethod("OnAsk", flags);

            var onTellHasAttributeApplied = onTell != null && Has<AlwaysInterleaveAttribute>(onTell);
            var onAskHasAttributeApplied = onAsk != null && Has<AlwaysInterleaveAttribute>(onAsk);

            if (onTellHasAttributeApplied && onAskHasAttributeApplied)
                return Both;

            if (onTellHasAttributeApplied)
                return Tell;

            if (onAskHasAttributeApplied)
                return Ask;

            return Default;
        }
    }
}
