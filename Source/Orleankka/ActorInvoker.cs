using System.Threading.Tasks;

namespace Orleankka
{
    public interface IActorInvoker
    {
        Task<object> OnReceive(object message);

        Task OnActivate();
        Task OnDeactivate();

        Task OnReminder(string id);
    }
}