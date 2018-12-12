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
            var grain = registeredActivator.Create(context);
            
            if (grain is ActorGrain actor)
                actor.Initialize(context.ActivationServices, context.GrainIdentity.PrimaryKeyString);

            return grain;
        }

        public void Release(IGrainActivationContext context, object grain) => 
            registeredActivator.Release(context, grain);
    }
}