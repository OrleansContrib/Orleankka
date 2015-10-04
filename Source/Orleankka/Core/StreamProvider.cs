using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleans;
using Orleans.Streams;
using Orleans.Providers;
using Orleans.Runtime.Configuration;

namespace Orleankka.Core
{
    using Client;
    using Cluster;
    using Utility;

    class StreamProvider : IStreamProviderImpl
    {
        static readonly string[] Separator = {":"};

        static readonly Dictionary<string, List<StreamSubscriptionSpecification>> configuration = 
                    new Dictionary<string, List<StreamSubscriptionSpecification>>();

        internal static void Reset() => configuration.Clear();

        internal static void Register(ActorType type)
        {
            var attributes = type.Implementation.GetCustomAttributes<StreamSubscriptionAttribute>(inherit: true);

            foreach (var attribute in attributes)
            {
                if (string.IsNullOrWhiteSpace(attribute.Source))
                    throw new InvalidOperationException($"StreamSubscription attribute defined on '{type.Implementation}' " +
                                                         "has null or whitespace only value of Source");

                if (string.IsNullOrWhiteSpace(attribute.Target))
                    throw new InvalidOperationException($"StreamSubscription attribute defined on '{type.Implementation}' " +
                                                         "has null or whitespace only value of Target");

                var parts = attribute.Source.Split(Separator, 2, StringSplitOptions.None);
                if (parts.Length != 2)
                    throw new InvalidOperationException($"StreamSubscription attribute defined on '{type.Implementation}' " +
                                                         "has invalid Source specification: " + attribute.Source);
                var provider = parts[0];
                var source = parts[1];

                var specifications = configuration.Find(provider);
                if (specifications == null)
                {
                    specifications = new List<StreamSubscriptionSpecification>();
                    configuration.Add(provider, specifications);
                }

                var specification = new StreamSubscriptionSpecification(source, attribute.Target, type.Implementation);
                specifications.Add(specification);
            }
        }

        internal const string TypeKey = "<-::Type::->";

        List<StreamSubscriptionSpecification> specifications;
        IStreamProviderImpl provider;
        IActorSystem system;

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;

            system = ClusterActorSystem.Initialized 
                ? ClusterActorSystem.Current 
                : ClientActorSystem.Current;

            specifications = configuration.Find(name)
                ?? new List<StreamSubscriptionSpecification>();

            var type = Type.GetType(config.Properties[TypeKey]);
            config.RemoveProperty(TypeKey);
            Debug.Assert(type != null);

            provider = (IStreamProviderImpl)Activator.CreateInstance(type);
            return provider.Init(name, providerRuntime, config);
        }

        public string Name { get; private set; }
        public bool IsRewindable => provider.IsRewindable;

        public Task Start() => provider.Start();
        public Task Stop() => provider.Stop();

        public IAsyncStream<T> GetStream<T>(Guid unused, string id)
        {
            var recipients = specifications
                .Where(x => x.Matches(id))
                .Select(x => x.Target(system, id))
                .ToArray();

            Func<T, Task> fan = item => TaskDone.Done;

            if (recipients.Length != 0)
                fan = item => Task.WhenAll(recipients.Select(x => x.Tell(item)));

            return new Stream<T>(provider.GetStream<T>(unused, id), fan);
        }

        class Stream<T> : IAsyncStream<T>
        {
            readonly IAsyncStream<T> stream;
            readonly Func<T, Task> fan;

            public Stream(IAsyncStream<T> stream, Func<T, Task> fan)
            {
                this.stream = stream;
                this.fan = fan;
            }

            public Task OnNextAsync(T item, StreamSequenceToken token = null)
            {
                return Task.WhenAll(stream.OnNextAsync(item, token), fan(item));
            }

            #region Uninteresting Delegation (Nothing To See Here)

            public Guid Guid => stream.Guid;
            public string Namespace => stream.Namespace;
            public bool Equals(IAsyncStream<T> other) => stream.Equals(other);
            public int CompareTo(IAsyncStream<T> other) => stream.CompareTo(other);
            public Task<StreamSubscriptionHandle<T>> SubscribeAsync(IAsyncObserver<T> observer) => stream.SubscribeAsync(observer);

            public Task<StreamSubscriptionHandle<T>> SubscribeAsync(
                IAsyncObserver<T> observer,
                StreamSequenceToken token,
                StreamFilterPredicate filterFunc = null,
                object filterData = null) => stream.SubscribeAsync(observer, token, filterFunc, filterData);

            public Task OnCompletedAsync() => stream.OnCompletedAsync();
            public Task OnErrorAsync(Exception ex) => stream.OnErrorAsync(ex);
            public Task OnNextBatchAsync(IEnumerable<T> batch, StreamSequenceToken token = null) => stream.OnNextBatchAsync(batch, token);
            public Task<IList<StreamSubscriptionHandle<T>>> GetAllSubscriptionHandles() => stream.GetAllSubscriptionHandles();
            public bool IsRewindable => stream.IsRewindable;
            public string ProviderName => stream.ProviderName;

            #endregion
        }

        class StreamSubscriptionSpecification
        {
            readonly string source;
            readonly string target;
            readonly Type actor;

            public StreamSubscriptionSpecification(string source, string target, Type actor)
            {
                this.source = source;
                this.target = target;
                this.actor = actor;
            }

            public bool Matches(string stream)
            {
                return source == stream;
            }

            public ActorRef Target(IActorSystem system, string stream)
            {
                return system.ActorOf(actor, target);
            }
        }
    }

    class StreamProviderConfiguration
    {
        readonly string name;
        readonly Type type;
        readonly IDictionary<string, string> properties;

        public StreamProviderConfiguration(string name, Type type, IDictionary<string, string> properties)
        {
            this.name = name;
            this.type = type;
            this.properties = properties ?? new Dictionary<string, string>();
        }

        public void Register(ClientConfiguration configuration)
        {
            properties.Add(StreamProvider.TypeKey, type.AssemblyQualifiedName);
            configuration.RegisterStreamProvider(typeof(StreamProvider).FullName, name, properties);
        }

        public void Register(ClusterConfiguration configuration)
        {
            properties.Add(StreamProvider.TypeKey, type.AssemblyQualifiedName);
            configuration.Globals.RegisterStreamProvider(typeof(StreamProvider).FullName, name, properties);
        }
    }
}