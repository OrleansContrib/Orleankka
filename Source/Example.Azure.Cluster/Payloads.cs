using System;
using System.Linq;

namespace Example.Azure
{
    [Serializable]
    public class Event
    {
        public readonly long Id;
        public readonly string Sender;
        public readonly DateTime Time;

        public Event()
        {}

        public Event(string sender, long id, DateTime time)
        {
            Id = id;
            Sender = sender;
            Time = time;
        }
    }

    [Serializable]
    public class Notification
    {
        public readonly Event Event;
        public readonly DateTime Received;
        public readonly string Hub;

        public Notification()
        {}

        public Notification(Event e, DateTime received, string hub)
        {
            Event = e;
            Received = received;
            Hub = hub;
        }
    }
}
