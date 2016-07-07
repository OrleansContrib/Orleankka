using System;
using System.Linq;

namespace Orleankka
{
    public struct Activate
    {}

    public struct Deactivate
    {}

    public struct Reminder
    {
        public readonly string Id;

        public Reminder(string id)
        {
            Id = id;
        }
    }

    public struct Timer
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