using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Dynamic
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public interface IDynamicActor : IGrainWithStringKey, IRemindable
    {
        Task OnTell(ActorPath path, byte[] message);
        Task<byte[]> OnAsk(ActorPath path, byte[] message);
    }
}
