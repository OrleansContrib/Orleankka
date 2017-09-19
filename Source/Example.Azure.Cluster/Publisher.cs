﻿using System;
using System.Threading.Tasks;

using Orleankka;

namespace Example.Azure
{
    public class Publisher : Actor, IPublisher
    {
        static readonly Random rand = new Random();

        void On(InitPublisher _) {}

        public override Task OnActivate()
        {
            Timers.Register("pub-pub", 
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
