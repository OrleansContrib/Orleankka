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

    public struct Reminder
    {
        public static readonly Reminder Invalid =
            new Reminder();

        public static Reminder Message(string name, TickStatus status) =>
            new Reminder(name, status);

        public string Name { get; }
        public TickStatus Status { get; }

        public Reminder(string name, TickStatus status = default(TickStatus))
        {
            Name = name;
            Status = status;
        }
    }
}