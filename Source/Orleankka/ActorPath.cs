using System;
using System.Diagnostics;
using System.Linq;

using Orleans;
using Orleans.Concurrency;

namespace Orleankka
{
    using Utility;
     
    [Serializable, Immutable]
    [DebuggerDisplay("{ToString()}")]
    public struct ActorPath : IEquatable<ActorPath>
    {
        public static readonly ActorPath Empty = new ActorPath();
        public static readonly string[] Separator = {":"};

        public static ActorPath For<T>(string id) where T : IActorGrain, IGrainWithStringKey => 
            For(typeof(T), id);
        
        public static ActorPath For(Type @interface, string id)
        {
            Requires.NotNull(@interface, nameof(@interface));
            Requires.NotNull(id, nameof(id));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An actor id cannot be empty or contain whitespace only", nameof(id));

            if (!typeof(IActorGrain).IsAssignableFrom(@interface))
                throw new InvalidOperationException($"Type '{@interface}' should be a type which implements IActorGrain interface");

            if (!@interface.IsInterface)
                @interface = GetActorInterface(@interface);

            return new ActorPath(@interface.FullName, id);
        }

        private static Type GetActorInterface(Type type)
        {
            var interfaces = type
                             .GetInterfaces().Except(new[] { typeof(IActorGrain) })
                             .Where(each => each.GetInterfaces().Contains(typeof(IActorGrain)))
                             .ToArray();

            if (interfaces.Length > 1)
                throw new InvalidOperationException("Type can only implement a single custom IActorGrain interface. Type: " + type.FullName);

            return interfaces.Length == 1 ? interfaces[0] : type;
        }

        public static ActorPath Parse(string path)
        {
            Requires.NotNull(path, nameof(path));

            var parts = path.Split(Separator, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new ArgumentException("Invalid actor path: " + path);

            var @interface = parts[0];
            var id = parts[1];

            return new ActorPath(@interface, id);
        }

        public readonly string Interface;
        public readonly string Id;

        ActorPath(string @interface, string id)
        {            
            Interface = @interface;
            Id = id;
        }

        public bool Equals(ActorPath other) => Interface == other.Interface && string.Equals(Id, other.Id);
        public override bool Equals(object obj) => obj is ActorPath && Equals((ActorPath)obj);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Interface?.GetHashCode() ?? 0) * 397) ^
                        (Id?.GetHashCode() ?? 0);
            }
        }

        public static implicit operator string(ActorPath arg) => arg.ToString();

        public static bool operator ==(ActorPath left, ActorPath right) => Equals(left, right);
        public static bool operator !=(ActorPath left, ActorPath right) => !Equals(left, right);

        public override string ToString() => $"{Interface}:{Id}";
    }
}