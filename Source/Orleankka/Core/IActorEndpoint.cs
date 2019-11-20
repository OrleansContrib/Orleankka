using System.Threading.Tasks;

using Orleans;
using Orleans.Concurrency;

#pragma warning disable 1591

namespace Orleankka.Core
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public interface IActorEndpoint : IGrainWithStringKey, IRemindable
    {
        // unused
        Task Autorun();

        Task<object> Receive(object message);
        Task ReceiveVoid(object message);

        [OneWay]
        Task Notify(object message);
    }
}
