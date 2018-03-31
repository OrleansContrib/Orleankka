namespace Orleankka.Behaviors
{
    public class Transition
    {
        public static readonly Transition Initial = new Transition(null, null);

        internal Transition(State from, State to)
        {
            From = from;
            To = to;
        }

        public State From { get; }
        public State To { get; }

        public override string ToString() => $"{From} -> {To}";
    }
}