using System;
using System.Linq;

using Orleans.Runtime;

namespace Orleankka.Core
{
    interface IActorLifecycleManager
    {
        void Initialize(IGrainActivationContext context);
        void Release(IGrainActivationContext context);
    }
}