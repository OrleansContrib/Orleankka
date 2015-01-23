using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.TestKit
{
    public class ActorRefStub : IActorRef
    {
        public Task Tell(object message)
        {
            return TaskDone.Done;
        }

        public Task<TResult> Ask<TResult>(object message)
        {
            return Task.FromResult(default(TResult));
        }
    }
}