using System;
using System.Linq;

using Orleankka.Dynamic.Internal;

namespace Orleankka.Dynamic
{
    class DynamicActorObserver : IActorObserver, IEquatable<DynamicActorObserver>
    {
        readonly IDynamicActorObserver observer;

        public DynamicActorObserver(IDynamicActorObserver observer)
        {
            this.observer = observer;
        }

        public void OnNext(Notification notification)
        {
            observer.OnNext(new DynamicNotification(notification.Source, notification.Message));
        }

        public bool Equals(DynamicActorObserver other)
        {
            return !ReferenceEquals(null, other)
                   && (ReferenceEquals(this, other)
                       || observer == other.observer);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj)
                   && (ReferenceEquals(this, obj)
                       || obj.GetType() == typeof(DynamicActorObserver)
                       && Equals((DynamicActorObserver)obj));
        }

        public override int GetHashCode()
        {
            return observer.GetHashCode();
        }

        public static bool operator ==(DynamicActorObserver left, DynamicActorObserver right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DynamicActorObserver left, DynamicActorObserver right)
        {
            return !Equals(left, right);
        }
    }
}