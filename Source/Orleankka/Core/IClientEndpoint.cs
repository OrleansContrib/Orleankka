using System;
using System.Linq;

using Orleans;

namespace Orleankka.Core
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public interface IClientEndpoint : IGrainObserver
    {
        void Receive(object message);
    }
}