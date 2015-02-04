using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Core.Hardcore
{
    public sealed class Blend : IEquatable<Blend>
    {
        public static readonly Blend Default = new Blend();

        readonly Flavor[] flavors =
        {
            Activation.Singleton,
            Concurrency.Default,
            Placement.Default,
            Delivery.Default,
            Interleave.Default
        };

        readonly string name;

        Blend()
        {}

        Blend(Flavor[] flavors)
        {
            this.flavors = flavors;
            name = Name(flavors);
        }

        static string Name(IEnumerable<Flavor> flavors)
        {
            return string.Join(".", flavors
                .Select(x => x.ToString())
                .Reverse()
                .SkipWhile(x => x == "Default")
                .Reverse());
        }

        public static Blend Mix(params Flavor[] flavors)
        {
            return flavors.Aggregate(Default, (current, flavor) => current.Mix(flavor));
        }

        public Blend Mix(Flavor flavor)
        {
            var mix = flavors.ToArray();

            for (int i = 0; i < flavors.Length; i++)
            {
                if (flavors[i].GetType() == flavor.GetType())
                    mix[i] = flavor;
            }

            return new Blend(mix);
        }

        public IEnumerable<Flavor> Flavors
        {
            get { return flavors; }
        }

        public static Blend From(Type type)
        {
            return Default
                    .Mix(Activation.Of(type))
                    .Mix(Concurrency.Of(type))
                    .Mix(Placement.Of(type))
                    .Mix(Delivery.Of(type))
                    .Mix(Interleave.Of(type));
        }

        public string GetInterfaceAttributeString()
        {
            return ToAttributeString(GetInterfaceAttributes());
        }

        public string GetClassAttributesString()
        {
            return ToAttributeString(GetClassAttributes());
        }

        public string GetTellMethodAttributesString()
        {
            return ToAttributeString(GetTellMethodAttributes());
        }

        public string GetAskMethodAttributeString()
        {
            return ToAttributeString(GetAskMethodAttributes());
        }

        static string ToAttributeString(IEnumerable<Flavor> attributes)
        {
            var atts = attributes as Flavor[] ?? attributes.ToArray();
            return atts.Any() ? "[" + string.Join(", ", atts.Select(x => x.AttributeText)) + "]" : "";
        }

        IEnumerable<Flavor> GetInterfaceAttributes()
        {
            return flavors.OfType<TypeLevelFlavor>().Where(x => x.AppliesToInterface()).ToArray();
        }

        IEnumerable<Flavor> GetClassAttributes()
        {
            return flavors.OfType<TypeLevelFlavor>().Where(x => x.AppliesToClass()).ToArray();
        }

        IEnumerable<Flavor> GetTellMethodAttributes()
        {
            return flavors.OfType<MethodLevelFlavor>().Where(x => x.AppliesToTell()).ToArray();
        }

        IEnumerable<Flavor> GetAskMethodAttributes()
        {
            return flavors.OfType<MethodLevelFlavor>().Where(x => x.AppliesToAsk()).ToArray();
        }

        public override string ToString()
        {
            return name;
        }

        public bool Equals(Blend other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || string.Equals(name, other.name));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) 
                    || obj is Blend && Equals((Blend) obj));
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public static bool operator ==(Blend left, Blend right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Blend left, Blend right)
        {
            return !Equals(left, right);
        }
    }
}
