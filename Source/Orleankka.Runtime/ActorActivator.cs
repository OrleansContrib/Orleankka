using Orleankka.Core;

namespace Orleankka
{
    using Orleans.Runtime;

    class ActorGrainActivator : IGrainActivator
    {
        readonly IGrainActivator registeredActivator;

        public ActorGrainActivator(IGrainActivator registeredActivator) => 
            this.registeredActivator = registeredActivator;

        public object Create(IGrainActivationContext context)
        {
            var instance = registeredActivator.Create(context);

            if (instance is ActorEndpoint endpoint)
                endpoint.Initialize(context);

            return instance;
        }

        public void Release(IGrainActivationContext context, object grain)
        {
            if (grain is ActorEndpoint endpoint)
                endpoint.Release(context);

            registeredActivator.Release(context, grain);
        }
    }
}