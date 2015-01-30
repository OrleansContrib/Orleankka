using System;
using System.Linq;

namespace Orleankka
{
    /// <summary>
    /// Represent actor notification data
    /// </summary>
    public sealed class Notification
    {
        /// <summary>
        /// The source actor that provides notification information.
        /// </summary>
        public readonly ActorRef Sender;

        /// <summary>
        /// The object that provides additional information about the notification.
        /// </summary>
        public readonly object Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="Notification"/> class.
        /// </summary>
        /// <param name="sender">The source actor that provides notification information.</param>
        /// <param name="message">The object that provides additional information about the notification.</param>
        public Notification(ActorRef sender, object message)
        {
            Requires.NotNull(sender, "sender");
            Requires.NotNull(message, "message");

            Sender = sender;
            Message = message;
        }
    }
}
