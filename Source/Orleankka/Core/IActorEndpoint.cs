using System.Threading.Tasks;

using Orleans;

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
