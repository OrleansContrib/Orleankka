using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    using Core;
    using Utility;

    public abstract class EndpointConfiguration : IEquatable<EndpointConfiguration>
    {
        static readonly Func<ActorPath, IActorRuntime, Func<object, Task<object>>> Null = 
            (path, runtime) => (message => TaskResult.Done);

        Func<object, bool> reentrancy = message => false;
        Func<ActorPath, IActorRuntime, Func<object, Task<object>>> receiver = Null;

        readonly HashSet<string> autoruns = new HashSet<string>(); 

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

        public Func<ActorPath, IActorRuntime, Func<object, Task<object>>> Receiver
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

        public void Autorun(params string[] ids)
        {
            Requires.NotNull(ids, nameof(ids));
            Array.ForEach(ids, x => autoruns.Add(x));
        }

        public string[] Autoruns => autoruns.ToArray();

        public bool Sticky { get; set; }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || 
                    obj.GetType() == GetType() && Equals((EndpointConfiguration) obj));
        }

        public bool Equals(EndpointConfiguration other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || 
                    string.Equals(Code, other.Code));
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