using System;
using System.Linq;

namespace Orleankka
{
    public interface IActorActivator
    {
        void Init(object properties);

        Actor Activate(Type type, string id, IActorRuntime runtime, Dispatcher dispatcher);
    }

    public abstract class ActorActivator<TProperties> : IActorActivator
    {
        void IActorActivator.Init(object properties)
        {
            Init((TProperties) properties);
        }

        public abstract void Init(TProperties properties);
        public abstract Actor Activate(Type type, string id, IActorRuntime runtime, Dispatcher dispatcher);
    }

    public abstract class ActorActivator : ActorActivator<object>
    {
        public override void Init(object properties) {}
    }

    class DefaultActorActivator : ActorActivator
    {
        public override Actor Activate(Type type, string id, IActorRuntime runtime, Dispatcher dispatcher)
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
