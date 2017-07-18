using System;

namespace Orleankka
{
    public interface IActorActivator
    {
        Actor Activate(Type type, string id, IActorRuntime runtime, Dispatcher dispatcher);
    }

    class DefaultActorActivator : IActorActivator
    {
        public Actor Activate(Type type, string id, IActorRuntime runtime, Dispatcher dispatcher)
        {
            try
            {
                return (Actor) Activator.CreateInstance(type, nonPublic: true);
            }
            catch (MissingMethodException)
            {
                throw new InvalidOperationException(
                    $"No parameterless constructor defined for {type} type");
            }
        }
    }
}
