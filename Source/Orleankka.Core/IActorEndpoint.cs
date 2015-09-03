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
        }

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Grain endpoint with Placement.Random
        /// </summary>
        public interface IA0 : IActorEndpoint
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Grain endpoint with Placement.PreferLocal
        /// </summary>
        public interface IA1 : IActorEndpoint
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Grain endpoint with Placement.DistributeEvenly
        /// </summary>
        public interface IA2 : IActorEndpoint
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Worker grain endpoint
        /// </summary>
        public interface IW : IActorEndpoint
        {}
    }
}
