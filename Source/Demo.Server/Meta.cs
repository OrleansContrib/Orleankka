using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;

namespace Demo
{
    public interface Command
    {}

    public interface Query
    {}

    public interface Query<TResult> : Query
    {}

    public interface Event
    {}

    public static class ActorRefExtensions
    {
        public static Task Send(this ActorRef @ref, Command cmd)
        {
            return @ref.Tell(cmd);
        }

        public static Task<TResult> Query<TResult>(this ActorRef @ref, Query<TResult> query)
        {
            return @ref.Ask<TResult>(query);
        }
    }
}
