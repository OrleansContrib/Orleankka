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

    public class DoAsk : Query<string>
    {
        public readonly ActorRef Target;
        public readonly object Message;

        public DoAsk(ActorRef target, object message)
        {
            Target = target;
            Message = message;
        }
    }

    public class ReceivedNotifications : Query<TextChanged[]>
    {}

    public class TestInsideActor : Actor
    {
        readonly List<TextChanged> notifications = new List<TextChanged>();

        public void Handle(TextChanged notification)
        {
            notifications.Add(notification);
        }

        public TextChanged[] Handle(ReceivedNotifications query)
        {
            return notifications.ToArray();
        }

        public async Task Handle(DoTell cmd)
        {
            await cmd.Target.Tell(cmd.Message);
        }

        public Task<string> Handle(DoAsk query)
        {
            return query.Target.Ask<string>(query.Message);
        }
    }
}
