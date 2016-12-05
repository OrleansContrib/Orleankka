using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans;

namespace Orleankka.Core
{
    using Utility;

    public class ActorInterface : IEquatable<ActorInterface>
    {
        static readonly Dictionary<string, ActorInterface> interfaces =
                    new Dictionary<string, ActorInterface>();

        internal static void Reset()
        {
            interfaces.Clear();
            ActorTypeName.Reset();
        }

        internal static void Register(IEnumerable<Assembly> assemblies, IEnumerable<ActorInterfaceMapping> mappings)
        {
            var generated = InterfaceDeclaration.Generate(assemblies, mappings);

            foreach (var @interface in generated)
                Register(@interface);
        }

        static void Register(ActorInterface @interface)
        {
            var registered = interfaces.Find(@interface.name);
            if (registered != null)
                throw new ArgumentException(
                    $"An actor with type '{@interface.name}' has been already registered");

            interfaces.Add(@interface.name, @interface);
        }

        readonly string name;
        readonly Type grain;
        readonly Func<string, object> factory;

        internal ActorInterface(ActorInterfaceMapping mapping, Type grain)
        {
            name = mapping.Name;
            Array.ForEach(mapping.Types, ActorTypeName.Register);

            this.grain = grain;
            factory = Bind(grain);
        }

        public Assembly GrainAssembly() => grain.Assembly;

        static Func<string, object> Bind(Type type)
        {
            var method = typeof(GrainFactory).GetMethod("GetGrain", new[] { typeof(string), typeof(string) });
            var invoker = method.MakeGenericMethod(type);
            var instance = Activator.CreateInstance(typeof(GrainFactory), nonPublic: true);
            return x => invoker.Invoke(instance, new object[] { x, null });
        }

        internal static IEnumerable<ActorInterface> Registered() => interfaces.Values.ToArray();

        internal static ActorInterface Registered(string name)
        {
            var result = interfaces.Find(name);
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map type '{name}' to the corresponding actor. " +
                     "Make sure that you've registered an actor type or the assembly containing this type");

            return result;
        }

        internal IActorEndpoint Proxy(ActorPath path) => (IActorEndpoint) factory(path.Id);

        public bool Equals(ActorInterface other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || string.Equals(name, other.name));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) 
                    || obj.GetType() == GetType() && Equals((ActorInterface) obj));
        }

        public static bool operator ==(ActorInterface left, ActorInterface right) => Equals(left, right);
        public static bool operator !=(ActorInterface left, ActorInterface right) => !Equals(left, right);

        public override int GetHashCode() => name.GetHashCode();
        public override string ToString() => name;
    }
}