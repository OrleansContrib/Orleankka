using System;
using Orleankka.Core;

using Orleans;
using Orleans.Streams;

namespace Orleankka.Cluster
{
    class ClusterActorSystem : ActorSystem
    {
        internal readonly ActorInvocationPipeline Pipeline;

        internal ClusterActorSystem(
            IStreamProviderManager streamProviderManager, 
            IGrainFactory grainFactory, 
            ActorInvocationPipeline pipeline, 
            IActorRefInvoker invoker = null)
            : base(streamProviderManager, grainFactory, invoker)
        {
            Pipeline = pipeline;
        }
    }
}