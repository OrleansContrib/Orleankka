namespace Orleankka.Behaviors
{
    public struct Transition
    {
        internal Transition(State from, State to)
        {
            From = from;
            To = to;
        }

        public State From { get; }
        public State To { get; }
    }
}