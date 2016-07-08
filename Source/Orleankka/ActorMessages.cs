namespace Orleankka
{
    public interface ActorMessage
    {}

    public class Activate : ActorMessage
    {}

    public class Deactivate : ActorMessage
    {}

    public class Reminder : ActorMessage
    {
        public readonly string Id;

        public Reminder(string id)
        {
            Id = id;
        }
    }

    public class Timer : ActorMessage
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