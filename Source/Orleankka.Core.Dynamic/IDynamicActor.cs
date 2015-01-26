using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Dynamic.Internal
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public interface IDynamicActor : IGrainWithStringKey, IRemindable
    {
        Task OnTell(DynamicRequest request);
        Task<DynamicResponse> OnAsk(DynamicRequest request);
    }
}
