using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Core
{
    public interface IInstanceActivator
    {
        void Init(IDictionary<string, string> properties);

        /// <summary>
        /// The activation function, which creates actual instances of <see cref="Actor"/>
        /// </summary>
        Actor Activate(Type type);
    }

    class DefaultInstanceActivator : IInstanceActivator
    {
        public void Init(IDictionary<string, string> properties)
        {}

        public Actor Activate(Type type)
        {
            return (Actor) Activator.CreateInstance(type);
        }
    }
}
