using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleankka
{
    using Utility;

    public class ActorConfiguration
    {
        readonly List<StreamSubscriptionSpecification> subscriptions = 
             new List<StreamSubscriptionSpecification>();

        Func<object, bool> reentrancy = message => false;
        Func<string, ActorContext, Func<IActorContext, object, Task<object>>> receiver;
        TimeSpan keepAliveTimeout = TimeSpan.Zero;

        public ActorConfiguration(string code)
        {
            Requires.NotNullOrWhitespace(code, nameof(code));
            Code = code;
        }

        public string Code
        {
            get;
        }

        public bool Worker
        {
            get; set;
        }

        public Placement Placement
        {
            get; set;
        }

        public Func<object, bool> Reentrancy
        {
            get { return reentrancy; }
            set
            {
                Requires.NotNull(value, nameof(value));
                reentrancy = value;
            }
        }

        public Func<string, ActorContext, Func<IActorContext, object, Task<object>>> Receiver
        {
            get { return receiver; }
            set
            {
                Requires.NotNull(value, nameof(value));
                receiver = value;
            }
        }

        public TimeSpan KeepAliveTimeout
        {
            get { return keepAliveTimeout; }
            set
            {
                if (value < TimeSpan.FromMinutes(1))
                    throw new ArgumentException(
                        "Minimum activation GC timeout is 1 minute", nameof(value));

                keepAliveTimeout = value;
            }
        }

        public IEnumerable<StreamSubscriptionSpecification> Subscriptions => subscriptions;

        public void Add(StreamSubscriptionSpecification subscription)
        {
            subscription.Code = Code;
            subscriptions.Add(subscription);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || 
                    obj.GetType() == GetType() && Equals((ActorConfiguration) obj));
        }

        public override int GetHashCode() => Code.GetHashCode();
        public override string ToString() => Code;
    }
}