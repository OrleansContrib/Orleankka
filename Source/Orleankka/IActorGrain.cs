using System.Threading.Tasks;

using Orleans;
using Orleans.Concurrency;

namespace Orleankka
{
    public interface IActor
    {
        Task<object> ReceiveAsk(object message);
        Task ReceiveTell(object message);
        [OneWay] Task ReceiveNotify(object message);
    }

    public interface IActorGrain : IActor, IGrainWithStringKey, IRemindable
    {}
}
