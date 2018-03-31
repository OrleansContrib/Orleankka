namespace Orleankka.Behaviors
{
    public interface BehaviorMessage
    {}

    public sealed class Become : BehaviorMessage, LifecycleMessage
    {
        Become(){}
        public static readonly Become Message = new Become();
    }

    public sealed class Unbecome : BehaviorMessage, LifecycleMessage
    {
        Unbecome(){}
        public static readonly Unbecome Message = new Unbecome();
    }
}