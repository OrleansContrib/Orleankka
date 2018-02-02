using Orleans;

namespace Orleankka
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public interface IClientEndpoint : IGrainObserver
    {
        void Receive(object message);
    }
}