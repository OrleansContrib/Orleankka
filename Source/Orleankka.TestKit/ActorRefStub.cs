using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.TestKit
{
    public class ActorRefStub : IActorRef
    {
        ActorPath IActorRef.Path
        {
            get { throw new NotImplementedException(); }
        }

        Task IActorRef.Tell(object message)
        {
            return TaskDone.Done;
        }

        Task<TResult> IActorRef.Ask<TResult>(object message)
        {
            return Task.FromResult(default(TResult));
        }
    }
}