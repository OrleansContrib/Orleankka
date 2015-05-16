using System;
using System.Linq;

namespace Orleankka.Core
{
    public interface IActorActivator
    {
        void Init(object properties);

        Actor Activate(Type type);
    }

    public abstract class DefaultActorActivator<TProperties> : IActorActivator
    {
        void IActorActivator.Init(object properties)
        {
            Init((TProperties) properties);
        }

        public abstract void Init(TProperties properties);
        public abstract Actor Activate(Type type);
    }

    class DefaultActorActivator : IActorActivator
    {
        public void Init(object properties)
        {}

        public Actor Activate(Type type)
        {
            return (Actor) Activator.CreateInstance(type, nonPublic: true);
        }
    }
}
