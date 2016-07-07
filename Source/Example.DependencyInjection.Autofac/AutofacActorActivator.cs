using System;

using Orleankka;
using Orleankka.CSharp;

using Autofac;

namespace Example
{
    public sealed class AutofacActorActivator : ActorActivator<Action<ContainerBuilder>>
    {
        IContainer container;

        public override void Init(Action<ContainerBuilder> setup)
        {
            if (setup == null)
                throw new ArgumentNullException(
                    nameof(setup), "Expected setup action of type Action<ContainerBuilder>");

            var builder = new ContainerBuilder();
            setup(builder);
            
            container = builder.Build();
        }

        public override Actor Activate(Type type, IActorContext context, Dispatcher dispatcher)
        {
            return (Actor) container.Resolve(type, 
                new TypedParameter(typeof(IActorContext), context), 
                new TypedParameter(typeof(Dispatcher), dispatcher));
        }
    }
}