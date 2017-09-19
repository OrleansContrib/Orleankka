namespace Orleankka.Behaviors
{
    struct Transition
    {
        public readonly CustomBehavior From;
        public readonly CustomBehavior To;

        public Transition(CustomBehavior from, CustomBehavior to)
        {
            From = from;
            To = to;
        }
    }
}