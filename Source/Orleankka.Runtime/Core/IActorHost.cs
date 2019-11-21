using System;
using System.Threading.Tasks;

namespace Orleankka.Core
{
    public interface IActorHost
    {
        Task<object> Receive(object message);
    }
}