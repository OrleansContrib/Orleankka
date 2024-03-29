﻿using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans;

namespace Orleankka.Client
{
    /// <summary>
    /// Client-side actor system interface
    /// </summary>
    public interface IClientActorSystem : IActorSystem
    {
        /// <summary>
        /// Creates new <see cref="IClientObservable"/>
        /// </summary>
        /// <returns>New instance of <see cref="IClientObservable"/></returns>
        IClientObservable CreateObservable();
    }

    /// <summary>
    /// Client-side actor system
    /// </summary>
    public sealed class ClientActorSystem : ActorSystem, IClientActorSystem
    {
        readonly IGrainFactory grainFactory;

        public ClientActorSystem(Assembly[] assemblies, IServiceProvider serviceProvider)
            : base(assemblies, serviceProvider)
        {
            grainFactory = serviceProvider.GetService<IClusterClient>();
        }

        /// <inheritdoc />
        public IClientObservable CreateObservable()
        {
            var proxy = ClientEndpoint.Create(grainFactory);
            return new ClientObservable(proxy);
        }
    }
}