﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Runtime;
using Orleans.Streams;

namespace Orleankka
{
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
        /// <typeparam name="TItem">The type of the stream item</typeparam>
        /// <returns>The stream reference</returns>
        StreamRef<TItem> StreamOf<TItem>(StreamPath path);

        /// <summary>
        /// Acquires the client reference for the given client path
        /// </summary>
        /// <param name="path">The path of the client observable</param>
        /// <returns>The client path</returns>
        ClientRef ClientOf(string path);
    }

    /// <summary>
    /// Runtime implementation of <see cref="T:Orleankka.IActorSystem" />
    /// </summary>
    public abstract class ActorSystem : IActorSystem
    {
        readonly Dictionary<string, ActorGrainInterface> interfaces =
             new Dictionary<string, ActorGrainInterface>();

        readonly IServiceProvider serviceProvider;
        readonly IGrainFactory grainFactory;
        readonly IActorRefMiddleware actorRefMiddleware;
        readonly IStreamRefMiddleware streamRefMiddleware;

        protected ActorSystem(Assembly[] assemblies, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.grainFactory = serviceProvider.GetService<IGrainFactory>();
            this.actorRefMiddleware = serviceProvider.GetService<IActorRefMiddleware>();
            this.streamRefMiddleware = serviceProvider.GetService<IStreamRefMiddleware>();

            Register(assemblies);
        }

        void Register(IEnumerable<Assembly> assemblies)
        {
            foreach (var each in assemblies.SelectMany(x => x.GetTypes().Where(IsActorGrain)))
            {
                var @interface = new ActorGrainInterface(each);
                interfaces.Add(each.FullName, @interface);
            }

            bool IsActorGrain(Type type)
            {
                return type.IsInterface && typeof(IActorGrain).IsAssignableFrom(type) && type.GetCustomAttribute<ActorGrainMarkerInterfaceAttribute>() == null;
            }
        }

        /// <inheritdoc />
        public ActorRef ActorOf(ActorPath path)
        {
            if (path == ActorPath.Empty)
                throw new ArgumentException("Actor path is empty", nameof(path));

            var @ref = new ActorRef(path);
            Init(@ref);

            return @ref;
        }

        internal void Init(ActorRef @ref)
        {
            var path = @ref.Path;

            if (!interfaces.TryGetValue(path.Interface, out ActorGrainInterface @interface))
                throw new Exception($"Can't find registered interface for '{path.Interface}'");

            @ref.endpoint = @interface.Proxy(path.Id, grainFactory);
            @ref.middleware = actorRefMiddleware;
        }
        
        /// <inheritdoc />
        public StreamRef<TItem> StreamOf<TItem>(StreamPath path)
        {
            if (path == StreamPath.Empty)
                throw new ArgumentException("Stream path is empty", nameof(path));

            var @ref = new StreamRef<TItem>(path);
            Init(@ref);

            return @ref;
        }

        /// <inheritdoc />
        public ClientRef ClientOf(string path)
        {
            Requires.NotNullOrWhitespace(path, nameof(path));

            var @ref = new ClientRef(path);
            Init(@ref);

            return @ref;
        }

        internal void Init(ClientRef @ref)
        {
            var endpoint = ClientEndpoint.Proxy(@ref.Path, grainFactory);
            @ref.endpoint = endpoint;
        }

        public void Init<TItem>(StreamRef<TItem> @ref)
        {
            var provider = serviceProvider.GetRequiredKeyedService<IStreamProvider>(@ref.Path.Provider);
            @ref.provider = provider;
            @ref.middleware = streamRefMiddleware;
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
        /// <param name="interface">The actor interface</param>
        /// <param name="id">The actor id</param>
        /// <returns>An actor reference</returns>
        public static ActorRef ActorOf(this IActorSystem system, Type @interface, string id)
        {
            return system.ActorOf(ActorPath.For(@interface, id));
        }
        
        /// <summary>
        /// Acquires the actor reference for the given actor type and id.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        /// <param name="id">The actor id</param>
        /// <returns>An actor reference</returns>
        public static ActorRef ActorOf<TActor>(this IActorSystem system, string id) where TActor : IActorGrain, IGrainWithStringKey
        {
            return system.ActorOf(typeof(TActor), id);
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
        /// <param name="interface">The worker interface</param>
        /// <returns>An actor reference</returns>
        public static ActorRef WorkerOf(this IActorSystem system, Type @interface)
        {
            return system.ActorOf(ActorPath.For(@interface, "#"));
        }
        
        /// <summary>
        /// Acquires the actor reference for the given worker type.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        /// <returns>An actor reference</returns>
        public static ActorRef WorkerOf<TActor>(this IActorSystem system) where TActor : IActorGrain, IGrainWithStringKey
        {
            return system.WorkerOf(typeof(TActor));
        }

        /// <summary>
        /// Acquires the stream reference for the given provider name and id of the stream.
        /// </summary>
        /// <param name="system">The reference to actor system</param>
        /// <param name="provider">The name of the stream provider</param>
        /// <param name="id">The id</param>
        /// <typeparam name="TItem">The type of the stream item</typeparam>
        /// <returns>A stream reference</returns>
        public static StreamRef<TItem> StreamOf<TItem>(this IActorSystem system, string provider, string id)
        {
            return system.StreamOf<TItem>(StreamPath.From(provider, id));
        }

        /// <summary>
        /// Acquires the typed actor reference for the given id and type of the actor.
        /// The type could be either an interface or implementation class.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        /// <param name="id">The id</param>
        public static ActorRef<TActor> TypedActorOf<TActor>(this IActorSystem system, string id) where TActor : IActorGrain, IGrainWithStringKey
        {
            return new ActorRef<TActor>(system.ActorOf(ActorPath.For(typeof(TActor), id)));
        }

        /// <summary>
        /// Acquires the typed actor reference for the given id and type of the worker actor.
        /// The type could be either an interface or implementation class.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        public static ActorRef<TActor> TypedWorkerOf<TActor>(this IActorSystem system) where TActor : IActorGrain, IGrainWithStringKey
        {
            return new ActorRef<TActor>(system.ActorOf(ActorPath.For(typeof(TActor), "#")));
        }
    }
}