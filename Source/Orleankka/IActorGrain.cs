using System;
using System.Threading.Tasks;

using Orleans;
using Orleans.Concurrency;

namespace Orleankka
{
    [ActorGrainMarkerInterface]
    public interface IActorGrain : IRemindable
    {
        Task<object> ReceiveAsk(object message);
        Task ReceiveTell(object message);
        [OneWay] Task ReceiveNotify(object message);
    }

    [AttributeUsage(AttributeTargets.Interface)]
    internal class ActorGrainMarkerInterfaceAttribute : Attribute {}
}
