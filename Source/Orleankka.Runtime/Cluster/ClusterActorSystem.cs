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
            IActorRefMiddleware middleware = null)
            : base(streamProviderManager, grainFactory, middleware)
        {
            Pipeline = pipeline;
        }
    }
}