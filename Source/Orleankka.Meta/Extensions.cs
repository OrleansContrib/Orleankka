using System.Threading.Tasks;

namespace Orleankka.Meta
{
    public static class Extensions
    {
        public static Task Tell<TCommand>(this ActorRef @ref, TCommand cmd) where TCommand : Command
        {
            return @ref.Tell(cmd);
        }

        public static Task Tell<TActor>(this ActorRef<TActor> @ref, Command<TActor> cmd) where TActor : IActor
        {
            return @ref.Tell(cmd);
        }

        public static Task<TResult> Ask<TResult>(this ActorRef @ref, Query<TResult> query)
        {
            return @ref.Ask<TResult>(query);
        }

        public static Task<TResult> Ask<TActor, TResult>(this ActorRef<TActor> @ref, Query<TActor, TResult> query) where TActor : IActor
        {
            return @ref.Ask<TResult>(query);
        }

        public static void Notify<TEvent>(this ObserverRef @ref, TEvent @event) where TEvent : Event
        {
            @ref.Notify(@event);
        }
    }
}