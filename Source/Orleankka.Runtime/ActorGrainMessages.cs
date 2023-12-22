using System;
using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    [GenerateSerializer, Immutable]
    public class Reminder
    {
        [Id(0)]
        public string Name { get; }
        [Id(1)]
        public TickStatus Status { get; }

        public Reminder(string name, TickStatus status = default)
        {
            Name = name;
            Status = status;
        }
    }    
    
    [GenerateSerializer, Immutable]
    public class Timer
    {
        [Id(0)]
        public string Id { get; }

        public Timer(string id) => Id = id;
    }
}