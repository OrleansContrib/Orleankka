using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Internal
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public interface IActorHost : IGrainWithStringKey, IRemindable
    {
        Task ReceiveTell(Request request);
        Task<Response> ReceiveAsk(Request request);
    }
}
