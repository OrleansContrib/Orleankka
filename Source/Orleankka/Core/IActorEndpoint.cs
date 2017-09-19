using System.Threading.Tasks;

using Orleans;
#pragma warning disable 1591

namespace Orleankka.Core
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public interface IActorEndpoint : IGrainWithStringKey, IRemindable
    {
        Task Autorun();

        Task<object> Receive(object message);
        Task ReceiveVoid(object message);
    }
}
