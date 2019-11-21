using System;

using Microsoft.Extensions.DependencyInjection;

using Orleankka;
using Orleans.Runtime;

namespace Demo
{
    class DI : IGrainActivator
    {
        readonly IServiceProvider services;
        readonly DefaultGrainActivator activator;

        public DI(IServiceProvider services)
        {
            this.services = services;
            activator = new DefaultGrainActivator(services);
        }

        public object Create(IGrainActivationContext context)
        {
            var type = context.GrainType;
            var id = context.GrainIdentity.PrimaryKeyString;

            if (!typeof(Actor).IsAssignableFrom(type))
                return activator.Create(context);

            if (type == typeof(Api))
                return new Api(new ObserverCollection(), ApiWorkerFactory.Create(id));

            if (type == typeof(Topic))
                return new Topic(services.GetService<ITopicStorage>());

            throw new InvalidOperationException($"Unknown actor type: {type}");
        }

        public void Release(IGrainActivationContext context, object grain)
        {
            if (!typeof(Actor).IsAssignableFrom(context.GrainType))
                activator.Release(context, grain);
        }
    }
}