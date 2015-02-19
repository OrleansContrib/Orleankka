using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Concurrency;

namespace Orleankka.Core
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public interface IActorEndpoint : IGrainWithStringKey, IRemindable
    {
        Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
        [AlwaysInterleave] Task<ResponseEnvelope> ReceiveInterleave(RequestEnvelope envelope);
    }
}
