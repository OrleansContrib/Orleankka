using Orleans.Runtime;

namespace Orleankka
{
    using Orleans.Metadata;

    class ActorGrainActivator : IConfigureGrainContext, IConfigureGrainContextProvider
    {
        public void Configure(IGrainContext context)
        {
            if (context.GrainInstance is ActorGrain actor)
                actor.Initialize(context.ActivationServices, context.GrainId.Key.ToString());
        }

        public bool TryGetConfigurator(GrainType grainType, GrainProperties properties, out IConfigureGrainContext configurator)
        {
            configurator = new ActorGrainActivator();
            return true;
        }
    }
}