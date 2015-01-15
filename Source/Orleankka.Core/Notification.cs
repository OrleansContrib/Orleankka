using System;
using System.Linq;

using Orleans.Concurrency;

namespace Orleankka
{
    /// <summary>
    /// Represent actor notification data
    /// </summary>
    [Immutable, Serializable]
    public sealed class Notification
    {
        /// <summary>
        /// The source actor that provides notification information.
        /// </summary>
        public readonly ActorPath Source;

        /// <summary>
        /// The object that provides additional information about the notification.
        /// </summary>
        public readonly object Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="Notification"/> class.
        /// </summary>
        /// <param name="source">The source actor that provides notification information.</param>
        /// <param name="message">The object that provides additional information about the notification.</param>
        public Notification(ActorPath source, object message)
        {
            Requires.NotNull(source, "source");
            Requires.NotNull(message, "message");

            Source = source;
            Message = message;
        }
    }
}
