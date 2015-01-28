using System;
using System.Linq;
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
        public static ActorRef ActorOf<TActor>(this IActorSystem system, string id)
        {
            return system.ActorOf(new ActorPath(typeof(TActor), id));
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

        static ActorSystem()
        {
            Activator = path => (Actor) System.Activator.CreateInstance(path.Type);
        }

        /// <summary>
        /// The activation function, which creates actual instances of <see cref="Actor"/>
        /// </summary>
        /// <remarks>
        /// By default expects type to have a public parameterless constructor 
        /// as a consequence of using standard  <see cref="System.Activator"/>
        /// </remarks>
        public static Func<ActorPath, Actor> Activator { get; set; }

        /// <summary>
        /// The serialization function, which serializes messages to byte[]
        /// </summary>
        /// <remarks>
        /// By default uses standard binary serialization provided by <see cref="BinaryFormatter"/>
        /// </remarks>
        public static Func<object, byte[]> Serializer
        {
            get { return Message.Serializer; }
            set { Message.Serializer = value; }
        }

        /// <summary>
        /// The deserialization function, which deserializes byte[] back to messages
        /// </summary>
        /// <remarks>
        /// By default uses standard binary serialization provided by 
        /// <see cref="BinaryFormatter"/></remarks>
        public static Func<byte[], object> Deserializer
        {
            get { return Message.Deserializer; }
            set { Message.Deserializer = value; }
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            Requires.NotNull(path, "path");

            if (Actor.IsCompatible(path.Type))
                return new ActorRef(path, Actor.Proxy(path));

            throw new ArgumentException("Path type should be either an interface which implements IActor or non-abstract type inherited from DynamicActor", "path");
        }

        IActorObserver IActorSystem.ObserverOf(ActorPath path)
        {
            Requires.NotNull(path, "path");

            if (ClientObservable.IsCompatible(path))
                return ClientObservable.Observer(path);

            if (Actor.IsCompatible(path.Type))
                return Actor.Observer(path);

            throw new InvalidOperationException("Can't bind " + path.Type);
        }
    }
}