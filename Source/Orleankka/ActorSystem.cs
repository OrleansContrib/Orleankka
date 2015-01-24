using System;
using System.Linq;

using Orleans.Runtime;

namespace Orleankka
{
    /// <summary>
    /// Serves as factory for acquiring/dehydrating actor/observer references from their paths.
    /// Also, in reverse, allows to get actor/observer paths from their references.
    /// </summary>
    public interface IActorSystem
    {
        /// <summary>
        /// Acquires the reference for the given actor path.
        /// </summary>
        /// <param name="path">The actor path</param>
        /// <returns>An actor reference</returns>
        IActorRef ActorOf(ActorPath path);

        /// <summary>
        /// Dehydrates the reference to <see cref="IActorObserver"/> from its <see cref="ActorObserverPath"/>.
        /// </summary>
        /// <param name="path">The path of actor observer.</param>
        /// <returns>The instance of <see cref="IActorObserver"/> </returns>
        IActorObserver ObserverOf(ActorObserverPath path);

        /// <summary>
        /// Retruns the path of the given <see cref="IActorObserver"/> reference.
        /// </summary>
        /// <param name="observer">The actor observer reference.</param>
        /// <returns>The isntance of <see cref="ActorObserverPath"/> </returns>
        ActorObserverPath PathOf(IActorObserver observer);
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
        
        IActorObserver IActorSystem.ObserverOf(ActorObserverPath path)
        {
            Requires.NotNull(path, "path");

            return ActorObserverFactory.ActorObserverReference.Cast(GrainReference.FromKeyString(path));
        }

        ActorObserverPath IActorSystem.PathOf(IActorObserver observer)
        {
            return new ActorObserverPath(((GrainReference)observer).ToKeyString());
        }
    }
}