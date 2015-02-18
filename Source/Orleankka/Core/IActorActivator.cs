using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Core
{
    public interface IActorActivator
    {
        void Init(IDictionary<string, string> properties);

        /// <summary>
        /// The activation function, which creates actual instances of <see cref="Actor"/>
        /// </summary>
        Actor Activate(Type type);
    }

    class DefaultActorActivator : IActorActivator
    {
        public void Init(IDictionary<string, string> properties)
        {}

        public Actor Activate(Type type)
        {
            return (Actor) Activator.CreateInstance(type);
        }
    }
}
