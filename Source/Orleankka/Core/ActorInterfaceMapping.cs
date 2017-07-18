using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Core
{
    class ActorInterfaceMapping : IEquatable<ActorInterfaceMapping>
    {
        public static ActorInterfaceMapping Of(string typeName) => new ActorInterfaceMapping(typeName);

        public static ActorInterfaceMapping Of(Type type)
        {
            var name = ActorTypeName.Of(type);
            var types = new List<Type> {type};

            if (type.IsClass)
            {
                var @interface = ActorTypeName.CustomInterface(type);
                if (@interface != null)
                    types.Add(@interface);                
            }

            if (type.IsInterface)
            {
                var classes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()
                    .Where(x => x.IsClass && type.IsAssignableFrom(x)))
                    .ToArray();

                if (classes.Length > 1)
                    throw new InvalidOperationException(
                        $"Custom actor interface [{type.FullName}] is implemented by " +
                        $"multiple classes: {string.Join(" ; ", classes.Select(x => x.ToString()))}");

                types.Add(classes[0]);                
            }

            return new ActorInterfaceMapping(name, types.ToArray());
        }

        public readonly string TypeName;
        public readonly Type[] Types;
        public readonly string Key;

        ActorInterfaceMapping(string typeName, params Type[] types)
        {
            TypeName = typeName;
            Types = types;
            Key = $"{typeName} -> {string.Join(";", types.OrderBy(x => x.AssemblyQualifiedName).Select(x => x.AssemblyQualifiedName))}";
        }

        public bool Equals(ActorInterfaceMapping other) => 
            !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || 
            string.Equals(Key, other.Key));

        public override bool Equals(object obj) => 
            !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || 
            obj.GetType() == GetType() && Equals((ActorInterfaceMapping) obj));

        public static bool operator ==(ActorInterfaceMapping left, ActorInterfaceMapping right) => Equals(left, right);
        public static bool operator !=(ActorInterfaceMapping left, ActorInterfaceMapping right) => !Equals(left, right);

        public override int GetHashCode() => Key.GetHashCode();
    }
}