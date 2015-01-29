using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

using Orleankka.Internal;

namespace Orleankka
{
    /// <summary>
    /// Serves as factory for acquiring actor/observer references from their paths.
    /// </summary>
    public interface IActorSystem
    {
        /// <summary>
        /// Acquires the reference for the given actor path.
        /// </summary>
        /// <param name="path">The actor path</param>
        /// <returns>An actor reference</returns>
        ActorRef ActorOf(ActorPath path);

        /// <summary>
        /// Acquires the reference to <see cref="IActorObserver"/> from its <see cref="ActorPath"/>.
        /// </summary>
        /// <param name="path">The path of actor observer.</param>
        /// <returns>The instance of <see cref="IObserver{T}"/> </returns>
        IActorObserver ObserverOf(ActorPath path);
    }

    /// <summary>
    /// The actor system extensions.
    /// </summary>
    public static class ActorSystemExtensions
    {
        /// <summary>
        /// Acquires the reference for the given id and type of the actor.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        /// <param name="id">The id</param>
        /// <returns>An actor reference</returns>
        public static ActorRef ActorOf<TActor>(this IActorSystem system, string id) where TActor : Actor
        {
            return system.ActorOf(ActorPath.From(typeof(TActor), id));
        }
    }

    /// <summary>
    /// Runtime implementation of <see cref="IActorSystem"/>
    /// </summary>
    public sealed class ActorSystem : IActorSystem
    {
        /// <summary>
        /// The static instance of <see cref="IActorSystem"/>
        /// </summary>
        public static readonly IActorSystem Instance = new ActorSystem();

        ActorSystem()
        {}

        /// <summary>
        /// The activation function, which creates actual instances of <see cref="Actor"/>
        /// </summary>
        /// <remarks>
        /// By default expects type to have a public parameterless constructor 
        /// as a consequence of using standard  <see cref="System.Activator"/>
        /// </remarks>
        public static Func<Type, Actor> Activator
        {
            get { return ActorHost.Activator; }
            set
            {
                Requires.NotNull(value, "value");
                ActorHost.Activator = value;
            }
        }

        /// <summary>
        /// The serialization function, which serializes messages to byte[]
        /// </summary>
        /// <remarks>
        /// By default uses standard binary serialization provided by <see cref="BinaryFormatter"/>
        /// </remarks>
        public static Func<object, byte[]> Serializer
        {
            get { return Payload.Serialize; }
            set
            {
                Requires.NotNull(value, "value");
                Payload.Serialize = value;
            }
        }

        /// <summary>
        /// The deserialization function, which deserializes byte[] back to messages
        /// </summary>
        /// <remarks>
        /// By default uses standard binary serialization provided by 
        /// <see cref="BinaryFormatter"/></remarks>
        public static Func<byte[], object> Deserializer
        {
            get { return Payload.Deserialize; }
            set
            {
                Requires.NotNull(value, "value");
                Payload.Deserialize = value;
            }
        }

        /// <summary>
        /// Registers actor types defined in the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public static void Register(Assembly assembly)
        {
            Requires.NotNull(assembly, "assembly");

            var types = assembly
                .GetTypes()
                .Where(x => 
                    !x.IsAbstract 
                    && typeof(Actor).IsAssignableFrom(x));

            foreach (var type in types)
                TypeCache.Register(type);
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            if (path == ActorPath.Empty)
                throw new ArgumentException("ActorPath is empty", "path");

            if (Actor.IsCompatible(RuntimeType(path)))
                return new ActorRef(path, Actor.Proxy(path));

            throw new ArgumentException("Path type should be a non-abstract type inherited from Actor class", "path");
        }

        IActorObserver IActorSystem.ObserverOf(ActorPath path)
        {
            if (path == ActorPath.Empty)
                throw new ArgumentException("ActorPath is empty", "path");

            if (ClientObservable.IsCompatible(path))
                return ClientObservable.Observer(path);

            if (Actor.IsCompatible(RuntimeType(path)))
                return Actor.Observer(path);

            throw new InvalidOperationException("Can't bind IActorObserver reference for the given path: " + path);
        }

        internal static Type RuntimeType(ActorPath path)
        {
            return TypeCache.Find(path.TypeCode);
        }

        class TypeCache
        {
            static readonly Dictionary<string, Type> cache =
                        new Dictionary<string, Type>();

            internal static void Register(Type type)
            {
                var typeCode = ActorPath.TypeCodeOf(type);

                if (cache.ContainsKey(typeCode))
                {
                    var existing = cache[typeCode];

                    if (existing != type)
                        throw new ArgumentException(
                            string.Format("The type {0} has been already registered under the code {1}. Use TypeCodeOverride attribute to provide unique code for {2}",
                                          existing.FullName, typeCode, type.FullName));

                    throw new ArgumentException(string.Format("The type {0} has been already registered", type));
                }

                cache.Add(typeCode, type);
            }

            public static Type Find(string typeCode)
            {
                Type type = cache.Find(typeCode);
                
                if (type == null)
                    throw new InvalidOperationException(
                        string.Format("Unable to map type code '{0}' to the corresponding runtime type. Make sure that you've registered the assembly containing this type", typeCode));

                return type;
            }
        }
    }
}