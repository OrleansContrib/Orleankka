using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Codegen
{
    public sealed class ActorEndpointDeclaration : IEquatable<ActorEndpointDeclaration>
    {
        public static readonly IEnumerable<ActorEndpointDeclaration> AllPossibleDeclarations;
        public static readonly ActorEndpointDeclaration Default = new ActorEndpointDeclaration();

        static ActorEndpointDeclaration()
        {
            var all = new List<ActorEndpointDeclaration>();

            Mix(new[]
            {
                ActorEndpointAttributeCategory.Of<PlacementAttribute>(),
                ActorEndpointAttributeCategory.Of<DeliveryAttribute>(),
            },
            Default.Mix(ActivationAttribute.Actor), all);

            Mix(new[]
            {
                ActorEndpointAttributeCategory.Of<DeliveryAttribute>(),
            },
            Default.Mix(ActivationAttribute.Worker), all);

            AllPossibleDeclarations = all
                .Distinct()
                .ToList();
        }

        static void Mix(ICollection<ActorEndpointAttributeCategory> categories, ActorEndpointDeclaration prototype, ICollection<ActorEndpointDeclaration> result)
        {
            if (categories.Count == 0)
                return;

            var category = categories.First();
            var blends = category.Select(prototype.Mix);

            foreach (var blend in blends)
            {
                result.Add(blend);

                var next = categories
                    .Skip(1)
                    .ToArray();

                Mix(next, blend, result);
            }
        }

        readonly ActorEndpointAttribute[] attributes =
        {
            ActivationAttribute.Actor,
            PlacementAttribute.Auto,
            DeliveryAttribute.Ordered,
        };

        readonly string name;

        ActorEndpointDeclaration()
        {}

        ActorEndpointDeclaration(ActorEndpointAttribute[] attributes)
        {
            this.attributes = attributes;
            name = Name(attributes);
        }

        static string Name(IEnumerable<ActorEndpointAttribute> attributes)
        {
            return string.Join(".", attributes.Select(x => x.ToString()));
        }

        public static ActorEndpointDeclaration Mix(params ActorEndpointAttribute[] attributes)
        {
            return attributes.Aggregate(Default, (current, attribute) => current.Mix(attribute));
        }

        ActorEndpointDeclaration Mix(ActorEndpointAttribute attribute)
        {
            var mix = attributes.ToArray();

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].GetType() == attribute.GetType())
                    mix[i] = attribute;
            }

            return new ActorEndpointDeclaration(mix);
        }

        public static ActorEndpointDeclaration From(Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(ActorConfigurationAttribute), true);
            
            if (attributes.Length > 1)
                throw new InvalidOperationException(
                    string.Format("Type {0} has multiple actor configurations specified", type));

            return From(attributes.Length != 0 
                        ? ((ActorConfigurationAttribute) attributes[0]).Configuration 
                        : ActorConfiguration.Default);
        }

        static ActorEndpointDeclaration From(ActorConfiguration cfg)
        {
            return Default
                    .Mix(ActivationAttribute.Of(cfg))
                    .Mix(PlacementAttribute.Of(cfg))
                    .Mix(DeliveryAttribute.Of(cfg));
        }

        public string GetInterfaceAttributeString()
        {
            return ToAttributeString(GetInterfaceAttributes());
        }

        public string GetClassAttributesString()
        {
            return ToAttributeString(GetClassAttributes());
        }

        static string ToAttributeString(IEnumerable<ActorEndpointAttribute> attributes)
        {
            var atts = attributes as ActorEndpointAttribute[] ?? attributes.ToArray();
            return atts.Any() ? "[" + string.Join(", ", atts.Select(x => x.AttributeText)) + "]" : "";
        }

        IEnumerable<ActorEndpointAttribute> GetInterfaceAttributes()
        {
            return attributes.OfType<TypeAttribute>().Where(x => x.AppliesToInterface()).ToArray();
        }

        IEnumerable<ActorEndpointAttribute> GetClassAttributes()
        {
            return attributes.OfType<TypeAttribute>().Where(x => x.AppliesToClass()).ToArray();
        }

        public override string ToString()
        {
            return name;
        }

        public bool Equals(ActorEndpointDeclaration other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || string.Equals(name, other.name));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) 
                    || obj is ActorEndpointDeclaration && Equals((ActorEndpointDeclaration) obj));
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public static bool operator ==(ActorEndpointDeclaration left, ActorEndpointDeclaration right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ActorEndpointDeclaration left, ActorEndpointDeclaration right)
        {
            return !Equals(left, right);
        }
    }
}
