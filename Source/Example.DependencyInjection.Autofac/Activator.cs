using System;
using System.Collections.Generic;

using Orleankka;
using Orleankka.Core;

using Autofac;

namespace Example
{
    public class Activator : IActorActivator
    {
        IContainer container;

        public void Init(IDictionary<string, string> properties)
        {
            var x = new ContainerBuilder();

            x.RegisterType<SomeService>()
                .AsImplementedInterfaces()
                .WithParameter("connectionString", properties["ConnectionString"])
                .SingleInstance();

            x.RegisterType<DIActor>();

            container = x.Build();
        }

        public Actor Activate(Type type)
        {
            return container.Resolve(type) as Actor;
        }
    }
}
