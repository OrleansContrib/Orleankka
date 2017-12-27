using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    public interface IActor
    {
        Task Autorun();
        Task<object> Receive(object message);
        Task ReceiveVoid(object message);
        Task Notify(object message);
    }

    public interface IActorGrain : IActor, IGrainWithStringKey, IRemindable
    {}

    /// <summary>
    /// This for F# api
    /// </summary>
    /// <typeparam name="TMessage">Type of DU</typeparam>
    public interface IActorGrain<TMessage> : IActorGrain
    {}
}
