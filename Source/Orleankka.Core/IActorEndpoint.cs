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
        [AlwaysInterleave] Task<ResponseEnvelope> ReceiveReentrant(RequestEnvelope envelope);
    }

    namespace Static
    {
        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Actor with Placement.Random
        /// </summary>
        public interface IA0 : IActorEndpoint
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Actor with Placement.PreferLocal
        /// </summary>
        public interface IA1 : IActorEndpoint
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Actor with Placement.DistributeEvenly
        /// </summary>
        public interface IA2 : IActorEndpoint
        {}

        /// <summary>
        ///   FOR INTERNAL USE ONLY!
        ///   Worker
        /// </summary>
        public interface IW : IActorEndpoint
        {}
    }
}
