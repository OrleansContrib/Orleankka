using System;

namespace Orleankka
{
    /// <summary>
    /// Serves as factory for acquiring actor references.
    /// </summary>
    public interface IActorSystem : IDisposable
    {
        /// <summary>
        /// Acquires the actor reference for the given actor path.
        /// </summary>
        /// <param name="path">The path of the actor</param>
        /// <returns>The actor reference</returns>
        ActorRef ActorOf(ActorPath path);

        /// <summary>
        /// Acquires the stream reference for the given stream path
        /// </summary>
        /// <param name="path">The path of the stream</param>
        /// <returns>The stream reference</returns>
        StreamRef StreamOf(StreamPath path);
    }

    /// <summary>
    /// Runtime implementation of <see cref="IActorSystem"/>
    /// </summary>
    public abstract class ActorSystem : MarshalByRefObject, IActorSystem
    {
        public static IActorSystemConfigurator Configure()
        {
            return null;
        }

        public ActorRef ActorOf(ActorPath path)
        {
            if (path == ActorPath.Empty)
                throw new ArgumentException("Actor path is empty", nameof(path));

           return ActorRef.Deserialize(path);
        }

        public StreamRef StreamOf(StreamPath path)
        {
            if (path == StreamPath.Empty)
                throw new ArgumentException("Stream path is empty", nameof(path));

            return StreamRef.Deserialize(path);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public abstract void Dispose();
    }

    /// <summary>
    /// The actor system extensions.
    /// </summary>
    public static class ActorSystemExtensions
    {
        /// <summary>
        /// Acquires the actor reference for the given id and type of the actor.
        /// The type could be either an interface or implementation class.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        /// <param name="id">The id</param>
        /// <returns>An actor reference</returns>
        public static ActorRef ActorOf<TActor>(this IActorSystem system, string id) where TActor : Actor
        {
            return system.ActorOf(typeof(TActor).ToActorPath(id));
        }
        
        /// <summary>
        /// Acquires the actor reference for the given actor path string.
        /// </summary>
        /// <param name="system">The reference to actor system</param>
        /// <param name="path">The path string</param>
        /// <returns>An actor reference</returns>
        public static ActorRef ActorOf(this IActorSystem system, string path)
        {
            return system.ActorOf(ActorPath.Parse(path));
        }

        /// <summary>
        /// Acquires the stream reference for the given id and type of the stream.
        /// </summary>
        /// <param name="system">The reference to actor system</param>
        /// <param name="provider">The name of the stream provider</param>
        /// <param name="id">The id</param>
        /// <returns>A stream reference</returns>
        public static StreamRef StreamOf(this IActorSystem system, string provider, string id)
        {
            return system.StreamOf(StreamPath.From(provider, id));
        }
    }
}