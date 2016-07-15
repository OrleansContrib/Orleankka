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
            Task Autorun();

            Task<object> Receive(object message);
            [AlwaysInterleave] Task<object> ReceiveReentrant(object message);

            Task ReceiveVoid(object message);
            [AlwaysInterleave] Task ReceiveReentrantVoid(object message);
        }
    }
}
