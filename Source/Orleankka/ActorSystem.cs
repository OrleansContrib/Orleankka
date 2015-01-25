using System;
using System.Linq;

using Orleans.Runtime;

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
        /// <returns>The instance of <see cref="IActorObserver"/> </returns>
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
    /// Default implementation of <see cref="IActorSystem"/>
    /// </summary>
    public sealed class ActorSystem : IActorSystem
    {
        /// <summary>
        /// The static instance of <see cref="IActorSystem"/>
        /// </summary>
        public static readonly IActorSystem Instance = new ActorSystem();

        ActorSystem()
        {}

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            Requires.NotNull(path, "path");

            if (!IsStaticActor(path.Type))
                throw new ArgumentException("Path type should be an interface which implements IActor", "path");
            
            return new ActorRef(path, StaticActorFactory.Create(path));
        }
        
        IActorObserver IActorSystem.ObserverOf(ActorPath path)
        {
            Requires.NotNull(path, "path");

            if (path.Type == typeof(ClientObservable))
                return ActorObserverFactory.Cast(GrainReference.FromKeyString(path.Id));

            if (IsStaticActor(path.Type))
                return ActorObserverFactory.Cast(StaticActorFactory.Create(path));

            throw new InvalidOperationException("Can't bind " + path.Type);
        }

        static bool IsStaticActor(Type type)
        {
            return type.IsInterface && type != typeof(IActor) && typeof(IActor).IsAssignableFrom(type);
        }
    }
}