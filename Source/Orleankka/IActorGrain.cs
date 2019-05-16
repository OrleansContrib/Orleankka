using System;
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

    [ActorGrainMarkerInterface]
    public interface IActorGrain : IActor, IGrainWithStringKey, IRemindable
    {}

    [AttributeUsage(AttributeTargets.Interface)]
    internal class ActorGrainMarkerInterfaceAttribute : Attribute {}
}
