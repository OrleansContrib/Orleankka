using System;

namespace Orleankka.CSharp
{
    /// <summary>
    /// The actor system extensions.
    /// </summary>
    public static class ActorSystemExtensions
    {
        /// <summary>
        /// Acquires the actor reference for the given id and type of the actor.
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
        /// Acquires the typed actor reference for the given id and type of the actor.
        /// The type could be either an interface or implementation class.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        /// <param name="id">The id</param>
        public static ActorRef<TActor> TypedActorOf<TActor>(this IActorSystem system, string id) where TActor : IActor
        {
            return new ActorRef<TActor>(system.ActorOf(typeof(TActor).ToActorPath(id)));
        }

        /// <summary>
        /// Acquires the actor reference for the given worker type.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        /// <returns>An actor reference</returns>
        public static ActorRef WorkerOf<TActor>(this IActorSystem system) where TActor : Actor
        {
            return system.ActorOf(typeof(TActor).ToActorPath("#"));
        }

        /// <summary>
        /// Acquires the typed actor reference for the given id and type of the worker actor.
        /// The type could be either an interface or implementation class.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        /// <param name="id">The id</param>
        public static ActorRef<TActor> TypedWorkerOf<TActor>(this IActorSystem system) where TActor : IActor
        {
            return new ActorRef<TActor>(system.ActorOf(typeof(TActor).ToActorPath("#")));
        }
    }
}