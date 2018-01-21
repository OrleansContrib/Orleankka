namespace Orleankka.Behaviors
{
    public struct Transition
    {
        internal readonly CustomBehavior from;
        internal readonly CustomBehavior to;

        internal Transition(CustomBehavior from, CustomBehavior to)
        {
            this.from = from;
            this.to = to;
        }

        public string From => from.Name;
        public string To => to.Name;
    }
}