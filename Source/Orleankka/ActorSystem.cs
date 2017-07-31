using System;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Streams;

namespace Orleankka
{
    using Core;
    using Utility;

    /// <summary>
    /// Serves as factory for acquiring actor references.
    /// </summary>
    public interface IActorSystem
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
        
        /// <summary>
        /// Acquires the client reference for the given client path
        /// </summary>
        /// <param name="path">The path of the client observable</param>
        /// <returns>The client path</returns>
        ClientRef ClientOf(string path);
    }

    /// <summary>
    /// Runtime implementation of <see cref="IActorSystem"/>
    /// </summary>
    public abstract class ActorSystem : IActorSystem
    {
        protected IStreamProviderManager StreamProviderManager { get; private set; }
        protected IGrainFactory GrainFactory { get; private set; }

        protected void Initialize(IServiceProvider provider)
        {
            StreamProviderManager = provider.GetRequiredService<IStreamProviderManager>();
            GrainFactory = provider.GetRequiredService<IGrainFactory>();

            ActorInterface.Bind(GrainFactory);
        }

        /// <summary>
        /// Entry-point method for fluent configuration
        /// </summary>
        /// <returns>An instance of actor system configurator</returns>
        public static IActorSystemConfigurator Configure() => default(IActorSystemConfigurator);

        /// <inheritdoc />
        public ActorRef ActorOf(ActorPath path)
        {
            if (path == ActorPath.Empty)
                throw new ArgumentException("Actor path is empty", nameof(path));

            var @interface = ActorInterface.Of(path.Type);
            var proxy = @interface.Proxy(path.Id, GrainFactory);

            return new ActorRef(path, proxy);
        }
        
        /// <inheritdoc />
        public StreamRef StreamOf(StreamPath path)
        {
            if (path == StreamPath.Empty)
                throw new ArgumentException("Stream path is empty", nameof(path));

            var provider = StreamProviderManager.GetStreamProvider(path.Provider);
            return new StreamRef(path, provider);
        }

        /// <inheritdoc />
        public ClientRef ClientOf(string path)
        {
            Requires.NotNullOrWhitespace(path, nameof(path));

            var endpoint = ClientEndpoint.Proxy(path, GrainFactory);
            return new ClientRef(endpoint);
        }
    }

    /// <summary>
    /// The actor system extensions.
    /// </summary>
    public static class ActorSystemExtensions
    {
        /// <summary>
        /// Acquires the actor reference for the given actor type and id.
        /// </summary>
        /// <param name="system">The reference to actor system</param>
        /// <param name="type">The actor type</param>
        /// <param name="id">The actor id</param>
        /// <returns>An actor reference</returns>
        public static ActorRef ActorOf(this IActorSystem system, string type, string id)
        {
            return system.ActorOf(ActorPath.From(type, id));
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
        /// Acquires the actor reference for the given worker type.
        /// </summary>
        /// <param name="system">The reference to actor system</param>
        /// <param name="type">The type</param>
        /// <returns>An actor reference</returns>
        public static ActorRef WorkerOf(this IActorSystem system, string type)
        {
            return system.ActorOf(ActorPath.From(type, "#"));
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
        /// Acquires the typed actor reference for the given id and type of the worker actor.
        /// The type could be either an interface or implementation class.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        public static ActorRef<TActor> TypedWorkerOf<TActor>(this IActorSystem system) where TActor : IActor
        {
            return new ActorRef<TActor>(system.ActorOf(typeof(TActor).ToActorPath("#")));
        }
    }
}