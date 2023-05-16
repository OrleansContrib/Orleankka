using System;
using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    public class Reminder
    {
        public string Name { get; }
        public TickStatus Status { get; }

        public Reminder(string name, TickStatus status = default)
        {
            Name = name;
            Status = status;
        }
    }    
    
    [Serializable, Immutable]
    public class Timer
    {
        public string Id { get; }

        public Timer(string id) => Id = id;
    }
}