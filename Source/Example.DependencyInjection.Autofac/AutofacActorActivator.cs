using System;
using System.Linq;

using Orleankka;
using Orleankka.Core;

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
                    "setup", "Expected setup action of type Action<ContainerBuilder>");

            var builder = new ContainerBuilder();
            setup(builder);
            
            container = builder.Build();
        }

        public override Actor Activate(Type type, string id, IActorRuntime runtime)
        {
            return (Actor) container.Resolve(type, 
                new NamedParameter("id", id), 
                new TypedParameter(typeof(IActorRuntime), runtime));
        }
    }
}