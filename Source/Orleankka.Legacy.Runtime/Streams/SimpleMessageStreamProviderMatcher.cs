using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Streams;
using Orleans.Providers.Streams.SimpleMessageStream;

namespace Orleankka.Legacy.Streams
{
    class SimpleMessageStreamProviderMatcher : IStreamProvider
    {
        readonly ConditionalWeakTable<object, object> streams = 
             new ConditionalWeakTable<object, object>();

        readonly IEnumerable<StreamSubscriptionSpecification> specifications;
        readonly IStreamProvider provider;
        readonly IActorSystem system;

        public SimpleMessageStreamProviderMatcher(IServiceProvider services, string name)
        {
            system = services.GetRequiredService<IActorSystem>();

            var registry = services.GetRequiredService<StreamSubscriptionSpecificationRegistry>();
            specifications = registry.Find(name);

            provider = SimpleMessageStreamProvider.Create(services, name);
            Name = name;
        }

        public string Name { get; }
        public bool IsRewindable => provider.IsRewindable;

        public IAsyncStream<T> GetStream<T>(Guid unused, string id)
        {
            var stream = provider.GetStream<T>(unused, id);

            return (IAsyncStream<T>) streams.GetValue(stream, _ =>
            {
                var recipients = StreamSubscriptionSpecification.Match(system, id, specifications);

                Func<T, Task> fan = item => Task.CompletedTask;
                Func<IEnumerable<T>, Task> batchFan = batch => Task.CompletedTask;

                if (recipients.Length > 0)
                {
                    fan = item => Task.WhenAll(recipients.Select(x => x.Receive(item)));
                    batchFan = batch => Task.WhenAll(recipients.Select(x => x.Receive(batch)));
                }

                return new StreamVentilator<T>(stream, fan, batchFan);
            });
        }

        class StreamVentilator<T> : IAsyncStream<T>
        {
            readonly IAsyncStream<T> stream;
            readonly Func<T, Task> fan;
            readonly Func<IEnumerable<T>, Task> batchFan;

            public StreamVentilator(IAsyncStream<T> stream, Func<T, Task> fan, Func<IEnumerable<T>, Task> batchFan)
            {
                this.stream = stream;
                this.fan = fan;
                this.batchFan = batchFan;
            }

            public Task OnNextAsync(T item, StreamSequenceToken token = null)
            {
                return Task.WhenAll(stream.OnNextAsync(item, token), fan(item));
            }

            public Task OnNextBatchAsync(IEnumerable<T> batch, StreamSequenceToken token = null)
            {
                // ReSharper disable PossibleMultipleEnumeration
                return Task.WhenAll(stream.OnNextBatchAsync(batch, token), batchFan(batch));
                // ReSharper restore PossibleMultipleEnumeration
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

            public Task<StreamSubscriptionHandle<T>> SubscribeAsync(IAsyncBatchObserver<T> observer) => stream.SubscribeAsync(observer);
            public Task<StreamSubscriptionHandle<T>> SubscribeAsync(IAsyncBatchObserver<T> observer, StreamSequenceToken token) => stream.SubscribeAsync(observer, token);

            public Task OnCompletedAsync() => stream.OnCompletedAsync();
            public Task OnErrorAsync(Exception ex) => stream.OnErrorAsync(ex);
            public Task<IList<StreamSubscriptionHandle<T>>> GetAllSubscriptionHandles() => stream.GetAllSubscriptionHandles();
            public bool IsRewindable => stream.IsRewindable;
            public string ProviderName => stream.ProviderName;

            #endregion
        }
    }
}