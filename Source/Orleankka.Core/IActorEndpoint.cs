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
        public interface IActorEndpoint : IActor, IGrainWithStringKey, IRemindable
        {
            Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
            [AlwaysInterleave] Task<ResponseEnvelope> ReceiveReentrant(RequestEnvelope envelope);

            Task ReceiveVoid(RequestEnvelope envelope);
            [AlwaysInterleave] Task ReceiveReentrantVoid(RequestEnvelope envelope);
        }

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        /// </summary>
        public interface IFixedEndpoint
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Fixed grain endpoint with Placement.Random
        /// </summary>
        public interface IA0 : IActorEndpoint, IFixedEndpoint
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Fixed grain endpoint with Placement.PreferLocal
        /// </summary>
        public interface IA1 : IActorEndpoint, IFixedEndpoint
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Fixed grain endpoint with Placement.DistributeEvenly
        /// </summary>
        public interface IA2 : IActorEndpoint, IFixedEndpoint
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Fixed worker grain endpoint
        /// </summary>
        public interface IW : IActorEndpoint, IFixedEndpoint
        {}
    }
}
