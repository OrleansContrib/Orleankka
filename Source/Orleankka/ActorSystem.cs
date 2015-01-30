using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

using Orleankka.Core;

namespace Orleankka
{
    /// <summary>
    /// Serves as factory for acquiring actor/observer references from their paths.
    /// </summary>
    public interface IActorSystem
    {
        /// <summary>
        /// Acquires the actor reference for the given path.
        /// </summary>
        /// <param name="path">The path of the actor</param>
        /// <returns>The actor reference</returns>
        ActorRef ActorOf(ActorPath path);

        /// <summary>
        /// Acquires the obserer reference for the given path
        /// </summary>
        /// <param name="path">The path of the observer</param>
        /// <returns>The observer reference</returns>
        ObserverRef ObserverOf(ObserverPath path);
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
        { }

        /// <summary>
        /// The activation function, which creates actual instances of <see cref="Actor"/>
        /// </summary>
        /// <remarks>
        /// By default expects type to have a public parameterless constructor 
        /// as a consequence of using standard  <see cref="System.Activator"/>
        /// </remarks>
        public static Func<Type, Actor> Activator
        {
            get { return ActorEndpoint.Activator; }
            set
            {
                Requires.NotNull(value, "value");
                ActorEndpoint.Activator = value;
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
            get { return MessageEnvelope.Serializer; }
            set
            {
                Requires.NotNull(value, "value");
                MessageEnvelope.Serializer = value;
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
            get { return MessageEnvelope.Deserializer; }
            set
            {
                Requires.NotNull(value, "value");
                MessageEnvelope.Deserializer = value;
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
                ActorPath.Register(type, type.Name); // TODO: add support for TypeCode override
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            if (path == ActorPath.Empty)
                throw new ArgumentException("ActorPath is empty", "path");

            return new ActorRef(path, ActorEndpoint.Proxy(path));
        }

        ObserverRef IActorSystem.ObserverOf(ObserverPath path)
        {
            if (path == ObserverPath.Empty)
                throw new ArgumentException("ObserverPath is empty", "path");

            return new ObserverRef(path, ObserverEndpoint.Proxy(path));
        }
    }
}