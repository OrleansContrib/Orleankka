using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka.Core.Streams
{
    class StreamConsumer
    {
        readonly ActorRef actor;
        readonly Func<object, bool> filter;

        public StreamConsumer(ActorRef actor, Func<object, bool> filter)
        {
            this.actor = actor;
            this.filter = filter;
        }

        public GrainReference Reference  => actor;
        public Task Receive(object item) => filter(item) ? actor.Tell(item) : TaskDone.Done;
    }
}