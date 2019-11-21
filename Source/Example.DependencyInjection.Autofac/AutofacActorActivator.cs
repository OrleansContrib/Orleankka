using System;
using Orleankka;
using Autofac;

using Orleans.Runtime;

namespace Example
{
    public sealed class AutofacActorActivator : IGrainActivator
    {
        readonly IContainer container;
        readonly DefaultGrainActivator @default;

        public AutofacActorActivator(IServiceProvider services, Action<ContainerBuilder> setup)
        {
            @default = new DefaultGrainActivator(services);

            if (setup == null)
                throw new ArgumentNullException(
                    nameof(setup), "Expected setup action of type Action<ContainerBuilder>");

            var builder = new ContainerBuilder();
            setup(builder);
            
            container = builder.Build();
        }

        public object Create(IGrainActivationContext context)
        {
            return typeof(Actor).IsAssignableFrom(context.GrainType)
                    ? container.Resolve(context.GrainType)
                    : @default.Create(context);
        }

        public void Release(IGrainActivationContext context, object grain)
        {
            if (!typeof(Actor).IsAssignableFrom(context.GrainType))
                @default.Release(context, grain);
        }
    }
}