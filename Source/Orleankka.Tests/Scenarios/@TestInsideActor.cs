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

    public class DoAttach : Command
    {
        public readonly ActorRef Target;

        public DoAttach(ActorRef target)
        {
            Target = target;
        }
    }

    public class GetReceivedNotifications : Query<Notification[]>
    {}

    public class TestInsideActor : Actor
    {
        readonly List<Notification> received = new List<Notification>();

        public override Task OnTell(object message)
        {
            return this.Handle((dynamic)message);
        }

        public override async Task<object> OnAsk(object message)
        {
            return await this.Answer((dynamic)message);
        }

        public override void OnNext(Notification notification)
        {
            received.Add(notification);
        }

        public Task Handle(DoTell cmd)
        {
            return cmd.Target.Tell(cmd.Message);
        }

        public Task<string> Answer(DoAsk query)
        {
            return query.Target.Ask<string>(query.Message);
        }

        public Task<Notification[]> Answer(GetReceivedNotifications query)
        {
            return Task.FromResult(received.ToArray());
        }
    }
}
