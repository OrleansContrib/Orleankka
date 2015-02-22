using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka
{
    /// <summary>
    /// This  is a simple helper class for actors that need to manage observer susbscriptions.
    /// It provides methods for adding/removing observers and for notifying about particular notifications.
    /// </summary>
    public interface IObserverCollection : IEnumerable<ObserverRef>
    {
        /// <summary>
        /// Adds given observer subscription.
        /// </summary>
        /// <param name="observer">The observer proxy</param>
        /// <remarks>The operation is idempotent</remarks>
        void Add(ObserverRef observer);

        /// <summary>
        /// Removes given observer subscription.
        /// </summary>
        /// <param name="observer">The observer proxy</param>
        /// <remarks>The operation is idempotent</remarks>
        void Remove(ObserverRef observer);

        /// <summary>
        /// Notifies all observers with specified notification information.
        /// </summary>
        /// <param name="message">The notification information</param>
        void Notify(object message);
    }

    /// <summary>
    /// Default implementation of <see cref="IObserverCollection"/>
    /// </summary>
    public class ObserverCollection : IObserverCollection
    {
        readonly HashSet<ObserverRef> observers = new HashSet<ObserverRef>();
        readonly Func<ActorRef> sender;

        public ObserverCollection(Actor actor)
            : this(() => actor.Self)
        {}

        internal ObserverCollection(Func<ActorRef> sender)
        {
            this.sender = sender;
        }

        void IObserverCollection.Add(ObserverRef observer)
        {
            observers.Add(observer);
        }

        void IObserverCollection.Remove(ObserverRef observer)
        {
            observers.Remove(observer);
        }

        void IObserverCollection.Notify(object message)
        {
            var failed = new List<ObserverRef>();

            foreach (var observer in observers)
            {
                try
                {
                    observer.Notify(message);
                }
                catch (Exception)
                {
                    failed.Add(observer);
                }
            }

            observers.RemoveWhere(failed.Contains);
        }

        public IEnumerator<ObserverRef> GetEnumerator()
        {
            return observers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}