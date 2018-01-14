using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    public interface IActor
    {
        Task<object> ReceiveAsk(object message);
        Task ReceiveTell(object message);
        Task ReceiveNotify(object message);
    }

    public interface IActorGrain : IActor, IGrainWithStringKey, IRemindable
    {}
}
