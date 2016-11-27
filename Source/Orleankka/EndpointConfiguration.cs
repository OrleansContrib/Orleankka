using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka
{
    using Core;
    using Utility;

    public abstract class EndpointConfiguration : IEquatable<EndpointConfiguration>
    {
        bool reentrant;
        Func<object, bool> interleavePredicate;

        Func<ActorPath, IActorRuntime, Actor> activator = 
            (path, runtime) => { throw new InvalidOperationException("Actor activator function is not set"); };

        string invoker;

        readonly HashSet<string> autoruns = new HashSet<string>(); 

        readonly List<StreamSubscriptionSpecification> subscriptions =
             new List<StreamSubscriptionSpecification>();

        TimeSpan keepAliveTimeout = TimeSpan.Zero;
        
        protected EndpointConfiguration(string type)
        {
            Requires.NotNullOrWhitespace(type, nameof(type));

            if (!EndpointDeclaration.IsValidIdentifier(type))
                throw new ArgumentException($"'{type}' is not valid identifier", nameof(type));

            Type = type;
        }

        public string Type { get;}

        public bool Reentrant
        {
            get { return reentrant; }
            set
            {
                if (value && interleavePredicate != null)
                    throw new InvalidOperationException(
                        $"'{Type}' actor can be designated either as fully reentrant or " +
                        "as partially reentrant (by specifying interleave predicate)");

                reentrant = value;
            }
        }

        public Func<object, bool> InterleavePredicate
        {
            get { return interleavePredicate; }
            set
            {
                if (Reentrant && value != null)
                    throw new InvalidOperationException(
                        $"'{Type}' actor can be designated either as fully reentrant or " +
                        "as partially reentrant (by specifying interleave predicate)");

                interleavePredicate = value;
            }
        }       

        public Func<ActorPath, IActorRuntime, Actor> Activator
        {
            get { return activator; }
            set
            {
                Requires.NotNull(value, nameof(value));
                activator = value;
            }
        }

        public string Invoker
        {
            get { return invoker; }
            set
            {
                Requires.NotNullOrWhitespace(value, nameof(value));
                invoker = value;
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

            subscription.Type = Type;
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
                    string.Equals(Type, other.Type));
        }

        public override int GetHashCode() => Type.GetHashCode();
        public override string ToString() => Type;

        internal abstract EndpointDeclaration Declaration();
    }

    public class WorkerConfiguration : EndpointConfiguration
    {
        public WorkerConfiguration(string type)
            : base(type)
        {}

        internal override EndpointDeclaration Declaration() => new WorkerDeclaration(this);
    }

    public class ActorConfiguration : EndpointConfiguration
    {
        public ActorConfiguration(string type) 
            : base(type)
        {}

        public Placement Placement
        {
            get; set;
        }

        internal override EndpointDeclaration Declaration() => new ActorDeclaration(this);
    }
}