using System.Threading.Tasks;

namespace Orleankka.Meta
{
    namespace CSharp
    {
        public interface Command<TActor> : ActorMessage<TActor>, Command where TActor : IActor		
        {}

        public interface Query<TActor, TResult> : ActorMessage<TActor, TResult>, Query<TResult> where TActor : IActor		
        {}

        public static class ActorRefExtensions
        {
            public static Task Tell<TActor>(this ActorRef<TActor> @ref, Command<TActor> cmd) where TActor : IActor
            {
                return @ref.Tell(cmd);
            }

            public static Task<TResult> Ask<TActor, TResult>(this ActorRef<TActor> @ref, Query<TActor, TResult> query) where TActor : IActor
            {
                return @ref.Ask(query);
            }
        }
    }
}