using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Internal
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public interface IActor : IGrainWithStringKey, IRemindable
    {
        Task OnTell(Request request);
        Task<Response> OnAsk(Request request);
    }
}
