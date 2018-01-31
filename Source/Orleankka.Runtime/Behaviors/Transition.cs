namespace Orleankka.Behaviors
{
    public struct Transition
    {
        internal readonly Receive from;
        internal readonly Receive to;

        internal Transition(Receive from, Receive to)
        {
            this.from = from;
            this.to = to;
        }

        public string From => from.Method.Name;
        public string To => to.Method.Name;
    }
}