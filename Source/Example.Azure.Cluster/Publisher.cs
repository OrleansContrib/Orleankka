using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Services;

namespace Example.Azure
{
    [Serializable]
    public class InitPublisher {}

    public class Publisher : Actor
    {
        static readonly Random rand = new Random();
        readonly ITimerService timers;

        public Publisher()
        {
            timers = new TimerService(this);
        }

        public void Handle(InitPublisher req)
        {}

        protected override Task OnActivate()
        {
            timers.Register("pub-pub", 
                TimeSpan.FromSeconds(1), 
                TimeSpan.FromSeconds(rand.Next(3, 10)), 
                () => HubGateway.Publish(Event()));

            return base.OnActivate();
        }

        Event Event()
        {
            var senderId = Id + "##" + HubGateway.LocalAddress();
            var eventId = DateTime.Now.Ticks ^ Id.GetHashCode();
            return new Event(senderId, eventId, DateTime.Now);
        }
    }
}
