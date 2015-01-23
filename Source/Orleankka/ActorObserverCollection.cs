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
    public interface IActorObserverCollection : IEnumerable<IActorObserver>
    {
        /// <summary>
        /// Adds given observer subscription.
        /// </summary>
        /// <param name="observer">The observer proxy</param>
        /// <remarks>The operation is idempotent</remarks>
        void Add(IActorObserver observer);

        /// <summary>
        /// Removes given observer subscription.
        /// </summary>
        /// <param name="observer">The observer proxy</param>
        /// <remarks>The operation is idempotent</remarks>
        void Remove(IActorObserver observer);

        /// <summary>
        /// Notifies all observers with specified notification information.
        /// </summary>
        /// <param name="message">The notification information</param>
        void Notify(object message);
    }

    /// <summary>
    /// Default implementation of <see cref="IActorObserverCollection"/>
    /// </summary>
    public class ActorObserverCollection : IActorObserverCollection
    {
        readonly HashSet<IActorObserver> observers = new HashSet<IActorObserver>();
        readonly Func<ActorPath> source;

        public ActorObserverCollection(Func<ActorPath> source)
        {
            this.source = source;
        }

        void IActorObserverCollection.Add(IActorObserver observer)
        {
            observers.Add(observer);
        }

        void IActorObserverCollection.Remove(IActorObserver observer)
        {
            observers.Remove(observer);
        }

        void IActorObserverCollection.Notify(object message)
        {
            var failed = new List<IActorObserver>();

            foreach (var observer in observers)
            {
                try
                {
                    observer.OnNext(new Notification(source(),  message));
                }
                catch (Exception)
                {
                    failed.Add(observer);
                }
            }

            observers.RemoveWhere(failed.Contains);
        }

        public IEnumerator<IActorObserver> GetEnumerator()
        {
            return observers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}