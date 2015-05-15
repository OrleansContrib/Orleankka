namespace Example
{
    using Autofac;
    using Orleankka;
    using Orleankka.Core;
    using System;
    using System.Collections.Generic;

    public class AutofacActorActivator : IActorActivator
    {
        public const string ContainerBuilderActionPropertyKey = "AutofacActorActivator:BuilderAction";

        private IContainer container;

        public void Init(IDictionary<string, object> properties)
        {
            if (!properties.ContainsKey(ContainerBuilderActionPropertyKey))
            {
                throw new InvalidOperationException(
                    "You must pass in a configurator action within the properties dictionary.");
            }

            var configureAction = properties[ContainerBuilderActionPropertyKey] as Action<ContainerBuilder>;

            if (configureAction == null)
            {
                throw new InvalidOperationException("The passed in action signature must be an Action<ContainerBuilder>.");
            }

            var builder = new ContainerBuilder();

            configureAction(builder);

            container = builder.Build();
        }

        public Actor Activate(Type type)
        {
            return container.Resolve(type) as Actor;
        }
    }
}
