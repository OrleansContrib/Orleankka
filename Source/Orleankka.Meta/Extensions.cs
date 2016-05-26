using System.Threading.Tasks;

namespace Orleankka.Meta
{
    public static class Extensions
    {
        public static Task Tell<TCommand>(this ActorRef @ref, TCommand cmd) where TCommand : Command
        {
            return @ref.Tell(cmd);
        }

        public static Task Tell<TActor>(this ActorRef @ref, Command<TActor> cmd) where TActor : Actor
        {
            return @ref.Tell(cmd);
        }

        public static Task<TResult> Ask<TResult>(this ActorRef @ref, Query<TResult> query)
        {
            return @ref.Ask<TResult>(query);
        }

        public static Task<TResult> Ask<TActor, TResult>(this ActorRef @ref, Query<TActor, TResult> query) where TActor : Actor
        {
            return @ref.Ask<TResult>(query);
        }

        public static void Notify<TEvent>(this ObserverRef @ref, TEvent @event) where TEvent : Event
        {
            @ref.Notify(@event);
        }
    }
}