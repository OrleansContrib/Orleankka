using System.Threading.Tasks;

namespace Orleankka
{
    public static class ActorRefExtensions
    {
        public static Task Activate(this ActorRef @ref) => @ref.Tell(Orleankka.Activate.External);
        public static Task Deactivate(this ActorRef @ref) => @ref.Tell(Orleankka.Deactivate.External);
    }
}