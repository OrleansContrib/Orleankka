using System;
using System.Linq;

using Orleans;

namespace Orleankka
{
    /// <summary>
    /// Provides a mechanism for receiving push-based notifications from actors.
    /// </summary>
    public interface IActorObserver : IGrainObserver
    {
        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="notification">The current notification information.</param>
        void OnNext(Notification notification);
    }
}