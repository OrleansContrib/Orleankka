using System;
using System.Collections.Generic;

namespace Orleankka.Core
{
    class ActorInterfaceMapping : IEquatable<ActorInterfaceMapping>
    {
        public static ActorInterfaceMapping Of(string name) => new ActorInterfaceMapping(name);

        public static ActorInterfaceMapping Of(Type type)
        {
            var name = ActorTypeName.Of(type);
            var types = new List<Type> {type};

            var @interface = ActorTypeName.CustomInterface(type);
            if (@interface != null)
                types.Add(@interface);

            return new ActorInterfaceMapping(name, types.ToArray());
        }

        public readonly string Name;
        public readonly Type[] Types;

        ActorInterfaceMapping(string name, params Type[] types)
        {
            Name = name;
            Types = types;
        }

        public bool Equals(ActorInterfaceMapping other) => 
            !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || 
            string.Equals(Name, other.Name));

        public override bool Equals(object obj) => 
            !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || 
            obj.GetType() == GetType() && Equals((ActorInterfaceMapping) obj));

        public static bool operator ==(ActorInterfaceMapping left, ActorInterfaceMapping right) => Equals(left, right);
        public static bool operator !=(ActorInterfaceMapping left, ActorInterfaceMapping right) => !Equals(left, right);

        public override int GetHashCode() => Name.GetHashCode();
    }
}