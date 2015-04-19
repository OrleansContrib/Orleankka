using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleankka;
using Orleankka.Services;

namespace Example.Azure
{
    [Serializable]
    public class PublishEvent
    {
        public Event Event;
    }

    [Reentrant(typeof(PublishEvent))]
    public class HubBuffer : Actor
    {
        readonly TimeSpan flushPeriod = TimeSpan.FromSeconds(1);        
        readonly Queue<Event> buffer = new Queue<Event>();
        readonly ITimerService timers;
        ActorRef hub;

        public HubBuffer()
        {
            timers = new TimerService(this);
        }

        protected override Task OnActivate()
        {
            hub = HubGateway.GetLocalHub();
            
            timers.Register("flush", flushPeriod, flushPeriod, Flush);
            
            return base.OnActivate();
        }

        Task Flush()
        {
            if (buffer.Count == 0)
                return TaskDone.Done;

            var events = buffer.ToArray();
            buffer.Clear();

            return hub.Tell(new PublishEvents{Events = events});
        }

        public void Handle(PublishEvent req)
        {
            buffer.Enqueue(req.Event);
        }
    }
}