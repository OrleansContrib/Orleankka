using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.TestKit
{
    public class ActorRefStub : ActorRef
    {
        public ActorRefStub(ActorPath path)
            : base(path)
        {}

        public override Task Tell(object message)
        {
            return TaskDone.Done;
        }

        public override Task<TResult> Ask<TResult>(object message)
        {
            return Task.FromResult(default(TResult));
        }
    }
}