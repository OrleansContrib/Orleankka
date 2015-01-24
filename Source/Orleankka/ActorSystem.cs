using System;
using System.Linq;

namespace Orleankka
{
    /// <summary>
    /// Serves as factory for actor references
    /// </summary>
    public interface IActorSystem
    {
        /// <summary>
        /// Acquires the reference for the given actor path.
        /// </summary>
        /// <param name="path">The actor path</param>
        /// <returns>An actor reference</returns>
        IActorRef ActorOf(ActorPath path);
    }

    /// <summary>
    /// The actor system extensions.
    /// </summary>
    public static class ActorSystemExtensions
    {
        /// <summary>
        /// Acquires the reference for the given id and interface type of the actor.
        /// </summary>
        /// <typeparam name="TActor">The interface type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        /// <param name="id">The id</param>
        /// <returns>An actor reference</returns>
        public static IActorRef ActorOf<TActor>(this IActorSystem system, string id) where TActor : IActor
        {
            return system.ActorOf(new ActorPath(typeof(TActor), id));
        }
    }

    /// <summary>
    /// Default implementation of <see cref="IActorSystem"/>
    /// </summary>
    public sealed class ActorSystem : IActorSystem
    {
        /// <summary>
        /// The static instance of <see cref="IActorSystem"/>
        /// </summary>
        public static readonly IActorSystem Instance = new ActorSystem();

        IActorRef IActorSystem.ActorOf(ActorPath path)
        {
            Requires.NotNull(path, "path");
            return new ActorRef(path);
        }
    }
}