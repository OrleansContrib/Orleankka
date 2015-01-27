using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.Actors
{
    public class TestInsideActor : Actor, ITestInsideActor
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
