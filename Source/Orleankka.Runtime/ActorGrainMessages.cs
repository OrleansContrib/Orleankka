using System;

using Orleans.Concurrency;
using Orleans.Runtime;

namespace Orleankka
{
    public interface LifecycleMessage
    {}

    public sealed class Activate : LifecycleMessage
    {
        public static readonly Activate Message = new Activate();
    }

    public sealed class Deactivate : LifecycleMessage
    {
        public static readonly Deactivate Message = new Deactivate();
    }

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