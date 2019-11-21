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

            if (instance is IActorLifecycleManager manager)
                manager.Initialize(context);

            return instance;
        }

        public void Release(IGrainActivationContext context, object grain)
        {
            if (grain is IActorLifecycleManager manager)
                manager.Release(context);

            registeredActivator.Release(context, grain);
        }
    }
}