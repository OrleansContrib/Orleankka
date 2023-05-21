using System;
using System.Diagnostics;

using Orleans;

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
        
        public static ActorPath For(Type interfaceOrClass, string id)
        {
            Requires.NotNull(interfaceOrClass, nameof(interfaceOrClass));
            Requires.NotNull(id, nameof(id));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An actor id cannot be empty or contain whitespace only", nameof(id));

            if (!typeof(IActorGrain).IsAssignableFrom(interfaceOrClass))
                throw new InvalidOperationException($"Type '{interfaceOrClass}' should be a type which implements IActorGrain interface");

            if (!interfaceOrClass.IsInterface)
                interfaceOrClass = ActorGrainInterface.InterfaceOf(interfaceOrClass);

            return new ActorPath(interfaceOrClass.FullName, id);
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