using System;
using System.Linq;

using Orleans;

namespace Orleankka.Dynamic
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public interface IDynamicActorObserver : IGrainObserver
    {
        void OnNext(DynamicNotification notification);
    }
}