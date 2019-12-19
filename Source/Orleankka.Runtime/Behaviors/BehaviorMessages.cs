namespace Orleankka.Behaviors
{
    public interface BehaviorMessage
    {}

    public class Become : BehaviorMessage, LifecycleMessage
    {
        protected Become(){}

        public static readonly Become Message = new Become();
    }

    public sealed class Become<TArg> : Become
    {
        public readonly TArg Argument;

        internal Become(TArg argument) => 
            Argument = argument;
    }

    public sealed class Unbecome : BehaviorMessage, LifecycleMessage
    {
        Unbecome(){}
        public static readonly Unbecome Message = new Unbecome();
    }
}