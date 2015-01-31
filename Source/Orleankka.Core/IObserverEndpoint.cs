using System;
using System.Linq;

using Orleans;

namespace Orleankka.Core
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public interface IObserverEndpoint : IGrainObserver
    {
        void ReceiveNotify(NotificationEnvelope envelope);
    }
}