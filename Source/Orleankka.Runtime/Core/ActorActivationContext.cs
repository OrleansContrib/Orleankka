using System;
using System.Linq;
using System.Collections.Generic;

using Orleans;
using Orleans.Core;
using Orleans.GrainDirectory;
using Orleans.Runtime;

namespace Orleankka.Core
{
    class ActorActivationContext : IGrainActivationContext
    {
        readonly IGrainActivationContext original;

        public ActorActivationContext(IGrainActivationContext original, Type actor)
        {
            this.original = original;
            GrainType = actor;
        }

        public Type GrainType { get; }
        public IGrainIdentity GrainIdentity => original.GrainIdentity;
        public IServiceProvider ActivationServices => original.ActivationServices;
        public Grain GrainInstance => original.GrainInstance;
        public IDictionary<object, object> Items => original.Items;
        public IGrainLifecycle ObservableLifecycle => original.ObservableLifecycle;
        public IMultiClusterRegistrationStrategy RegistrationStrategy => original.RegistrationStrategy;
    }
}