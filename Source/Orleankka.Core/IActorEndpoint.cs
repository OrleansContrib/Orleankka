using System.Threading.Tasks;

using Orleans;
using Orleans.Concurrency;

namespace Orleankka.Core
{
    namespace Endpoints
    {
        /// <summary> 
        /// FOR INTERNAL USE ONLY!
        /// </summary>
        public interface IActorEndpoint : IGrainWithStringKey, IRemindable
        {
            Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
            [AlwaysInterleave] Task<ResponseEnvelope> ReceiveReentrant(RequestEnvelope envelope);

            Task ReceiveVoid(RequestEnvelope envelope);
            [AlwaysInterleave] Task ReceiveReentrantVoid(RequestEnvelope envelope);
        }
    }
}
