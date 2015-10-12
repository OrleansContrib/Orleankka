using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Runtime;

namespace Orleankka.Core.Streams
{
    class StreamConsumer
    {
        readonly ActorRef actor;

        public StreamConsumer(ActorRef actor)
        {
            this.actor = actor;
        }

        public GrainReference Reference  => actor;
        public Task Receive(object item) => actor.Tell(item);
    }
}