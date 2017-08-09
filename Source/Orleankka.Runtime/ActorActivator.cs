using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

namespace Orleankka
{
    using Core;

    public interface IActorActivator
    {
        Actor Activate(Type type, string id, IActorRuntime runtime, Dispatcher dispatcher);
    }

    class DefaultActorActivator : IActorActivator
    {
        readonly IServiceProvider services;

        readonly Dictionary<Type, ObjectFactory> factories = 
             new Dictionary<Type, ObjectFactory>();

        public DefaultActorActivator(IServiceProvider services)
        {
            this.services = services;

            foreach (var type in ActorType.Registered())
                factories.Add(type.Class, ActivatorUtilities.CreateFactory(type.Class, Type.EmptyTypes));
        }

        public Actor Activate(Type type, string id, IActorRuntime runtime, Dispatcher dispatcher)
        {
            var factory = factories[type];
            return (Actor) factory(services, null);
        }
    }
}
