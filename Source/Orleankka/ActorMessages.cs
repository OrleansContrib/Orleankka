namespace Orleankka
{
    public interface SystemMessage
    {}

    public interface LifecycleMessage : SystemMessage
    {}

    public interface TickMessage : SystemMessage
    {}

    public class Activate : LifecycleMessage
    {}

    public class Deactivate : LifecycleMessage
    {}

    public class Reminder : TickMessage
    {
        public readonly string Id;

        public Reminder(string id)
        {
            Id = id;
        }
    }

    public class Timer : TickMessage
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