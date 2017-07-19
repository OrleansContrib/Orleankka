using System;
using System.Threading.Tasks;

using Orleans.Runtime;

namespace Orleankka.Core
{
    public interface IActorHost
    {
        IGrainRuntime Runtime { get; }

        Task<object> Receive(object message);
    }
}