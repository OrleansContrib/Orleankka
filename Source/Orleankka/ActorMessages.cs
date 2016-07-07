using System;
using System.Linq;

namespace Orleankka
{
    public class Activate
    {}

    public class Deactivate
    {}

    public class Reminder
    {
        public readonly string Id;

        public Reminder(string id)
        {
            Id = id;
        }
    }

    public class Timer
    {
        public readonly string Id;
        public readonly object State;

        public Timer(string id, object state = null)
        {
            Id = id;
            State = state;
        }
    }
}