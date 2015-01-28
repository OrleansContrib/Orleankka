using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.Dynamic.Actors
{
    public class DoTell : Command
    {
        public readonly ActorPath Path;
        public readonly object Message;

        public DoTell(ActorPath path, object message)
        {
            Path = path;
            Message = message;
        }
    }

    public class DoAsk : Query<object>
    {
        public readonly ActorPath Path;
        public readonly object Message;

        public DoAsk(ActorPath path, object message)
        {
            Path = path;
            Message = message;
        }
    }

    public class DoAttach : Command
    {
        public readonly ActorPath Path;

        public DoAttach(ActorPath path)
        {
            Path = path;
        }
    }

    [Serializable]
    public class GetReceivedNotifications : Query<Notification[]>
    {}

    public class TestInsideActor : DynamicActor
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
            return ActorOf(cmd.Path).Tell(cmd.Message);
        }

        public Task<string> Answer(DoAsk query)
        {
            return ActorOf(query.Path).Ask<string>(query.Message);
        }

        public Task Handle(DoAttach cmd)
        {
            return ActorOf(cmd.Path).Tell(new Attach(this));
        }

        public Task<Notification[]> Answer(GetReceivedNotifications query)
        {
            return Task.FromResult(received.ToArray());
        }
    }
}
