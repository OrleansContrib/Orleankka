using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.Scenarios
{
    public class DoTell : Command
    {
        public readonly ActorRef Target;
        public readonly object Message;

        public DoTell(ActorRef target, object message)
        {
            Target = target;
            Message = message;
        }
    }

    public class DoAsk : Query<object>
    {
        public readonly ActorRef Target;
        public readonly object Message;

        public DoAsk(ActorRef target, object message)
        {
            Target = target;
            Message = message;
        }
    }

    public class ReceivedNotifications : Query<object[]>
    {}

    public class TestInsideActor : Actor
    {
        readonly List<object> objects = new List<object>();

        public override Task<object> OnReceive(object message)
        {
            return this.Handle((dynamic)message);
        }

        public Task<object> Handle(object n)
        {
            objects.Add(n);
            return Done();
        }

        public Task<object> Handle(ReceivedNotifications q)
        {
            return Result(objects.ToArray());
        }

        public async Task<object> Handle(DoTell cmd)
        {
            await cmd.Target.Tell(cmd.Message);
            return Done();
        }

        public Task<object> Handle(DoAsk query)
        {
            return query.Target.Ask<object>(query.Message);
        }
    }
}
