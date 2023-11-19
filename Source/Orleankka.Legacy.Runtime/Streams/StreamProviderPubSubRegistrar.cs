using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;

namespace Orleankka.Legacy.Streams
{
    class StreamProviderPubSubRegistrar : ILifecycleParticipant<ISiloLifecycle>, ILifecycleObserver
    {
        public StreamProviderPubSubRegistrar(IServiceProvider serviceProvider, string name)
        {
            RegisterStreamSubscriptionPubSub(serviceProvider, name);
        }

        Task ILifecycleObserver.OnStart(CancellationToken _)
        {
            return Task.CompletedTask;
        }

        Task ILifecycleObserver.OnStop(CancellationToken _)
        {
            return Task.CompletedTask;
        }

        void ILifecycleParticipant<ISiloLifecycle>.Participate(ISiloLifecycle observer)
        {
            observer.Subscribe(nameof(StreamProviderPubSubRegistrar), ServiceLifecycleStage.AfterRuntimeGrainServices, this);
        }

        static void RegisterStreamSubscriptionPubSub(IServiceProvider provider, string streamProviderName)
        {
            var streamProvider = provider.GetServiceByName<IStreamProvider>(streamProviderName);

            var streamRuntime = GetStreamProviderRuntime(streamProvider);
            var orleansPubSubField = GetOrleansPubSubField(streamRuntime);
            var orleansPubSub = (IStreamPubSub)orleansPubSubField.GetValue(streamRuntime);

            var streamSubscriptionPubSub = ActivatorUtilities.CreateInstance<StreamSubscriptionPubSub>(provider);
            var compositePubSub = ActivatorUtilities.CreateInstance<CompositeStreamPubSub>(provider, orleansPubSub, streamSubscriptionPubSub);

            orleansPubSubField.SetValue(streamRuntime, compositePubSub);
        }

        static IProviderRuntime GetStreamProviderRuntime(IStreamProvider streamProvider)
        {
            var streamProviderType = streamProvider.GetType();
            var runtime = streamProviderType.GetField("runtime", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(runtime != null);
            return (IProviderRuntime)runtime.GetValue(streamProvider);
        }

        static FieldInfo GetOrleansPubSubField(IProviderRuntime streamProviderRuntime)
        {
            var streamProviderRuntimeType = streamProviderRuntime.GetType();
            var streamPubSub = streamProviderRuntimeType.GetField("combinedGrainBasedAndImplicitPubSub", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(streamPubSub != null);
            return streamPubSub;
        }
    }
}