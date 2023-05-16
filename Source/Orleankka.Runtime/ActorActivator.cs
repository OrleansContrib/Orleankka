namespace Orleankka
{
    using System.Threading.Tasks;

    using Orleans.Runtime;

    class ActorGrainActivator : IGrainActivator
    {
        readonly IGrainActivator registeredActivator;

        public ActorGrainActivator(IGrainActivator registeredActivator) => 
            this.registeredActivator = registeredActivator;

        public object CreateInstance(IGrainContext context)
        {
            var grain = registeredActivator.CreateInstance(context);
            
            if (grain is ActorGrain actor)
                actor.Initialize(context.ActivationServices, context.GrainId.ToString());

            return grain;
        }

        public ValueTask DisposeInstance(IGrainContext context, object instance) => 
            registeredActivator.DisposeInstance(context, instance);
    }
}