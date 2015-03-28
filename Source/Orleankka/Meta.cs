using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    namespace Meta
    {
        public interface Command
        {}

        public interface Query
        {}

        public interface Query<TResult> : Query
        {}

        public interface Event
        {}

        public static class Extensions
        {
            public static Task Tell<TCommand>(this ActorRef @ref, TCommand cmd) where TCommand : Command
            {
                return @ref.Tell(cmd);
            }

            public static Task<TResult> Ask<TResult>(this ActorRef @ref, Query<TResult> query)
            {
                return @ref.Ask<TResult>(query);
            }

            public static void Notify<TEvent>(this ObserverRef @ref, TEvent @event) where TEvent : Event
            {
                @ref.Notify(@event);
            }
        }
    }
}
