using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    public interface IActor : IGrainWithStringKey, IRemindable
    {
        Task OnTell(object message);
        Task<object> OnAsk(object message);
    }
}
