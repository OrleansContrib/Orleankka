using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Core
{
    public interface IActorActivator
    {
        void Init(IDictionary<string, object> properties);

        /// <summary>
        /// The activation function, which creates actual instances of <see cref="Actor"/>
        /// </summary>
        Actor Activate(Type type);
    }

    class DefaultActorActivator : IActorActivator
    {
        public void Init(IDictionary<string, object> properties)
        {}

        public Actor Activate(Type type)
        {
            return (Actor) Activator.CreateInstance(type, true);
        }
    }
}
