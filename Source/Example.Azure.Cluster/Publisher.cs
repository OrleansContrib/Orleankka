using System;

using Orleankka;
using Orleankka.CSharp;

namespace Example.Azure
{
    public class Publisher : Actor
    {
        static readonly Random rand = new Random();

        [Serializable]
        public class Init {}

        void On(Init _) {}

        void On(Activate _)
        {
            Timers.Register("pub-pub", 
                TimeSpan.FromSeconds(1), 
                TimeSpan.FromSeconds(rand.Next(3, 10)), 
                () => HubGateway.Publish(Event()));
        }

        Event Event()
        {
            var senderId = Id + "##" + HubGateway.LocalAddress();
            var eventId = DateTime.Now.Ticks ^ Id.GetHashCode();
            return new Event(senderId, eventId, DateTime.Now);
        }
    }
}
