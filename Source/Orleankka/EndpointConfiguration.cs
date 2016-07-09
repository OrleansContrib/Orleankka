using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;

namespace Orleankka
{
    using Core;
    using Utility;

    public abstract class EndpointConfiguration
    {
        static readonly Task<object> Done = Task.FromResult((object)null); 

        static readonly Func<IActorContext, Func<object, Task<object>>> Null = 
            (context) => (message => Done);

        Func<object, bool> reentrancy = message => false;
        Func<IActorContext, Func<object, Task<object>>> receiver = Null;

        readonly List<StreamSubscriptionSpecification> subscriptions =
             new List<StreamSubscriptionSpecification>();

        TimeSpan keepAliveTimeout = TimeSpan.Zero;

        protected EndpointConfiguration(string code)
        {
            Requires.NotNullOrWhitespace(code, nameof(code));

            if (!EndpointDeclaration.IsValidIdentifier(code))
                throw new ArgumentException($"'{code}' is not valid identifer", nameof(code));

            Code = code;
        }

        public string Code { get;}

        public Func<object, bool> Reentrancy
        {
            get { return reentrancy; }
            set
            {
                Requires.NotNull(value, nameof(value));
                reentrancy = value;
            }
        }

        public Func<IActorContext, Func<object, Task<object>>> Receiver
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
            Requires.NotNull(subscription, nameof(subscription));

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

        internal abstract EndpointDeclaration Declaration();
    }

    public class WorkerConfiguration : EndpointConfiguration
    {
        public WorkerConfiguration(string code)
            : base(code)
        {}

        internal override EndpointDeclaration Declaration() => new WorkerDeclaration(this);
    }

    public class ActorConfiguration : EndpointConfiguration
    {
        public ActorConfiguration(string code) 
            : base(code)
        {}

        public Placement Placement
        {
            get; set;
        }

        internal override EndpointDeclaration Declaration() => new ActorDeclaration(this);
    }
}