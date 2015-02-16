using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Core
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public interface IActorEndpoint : IGrainWithStringKey, IRemindable
    {
        Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
    }
}
