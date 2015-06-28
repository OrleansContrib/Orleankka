using System;
using System.Linq;

using Orleankka;
using Orleankka.Meta;

namespace Example.Serialization.Native
{
    [Serializable]
    public class Promote
    {
        public int NewLevel;
    }

    [Serializable]
    public class GetLevel : Query<int>
    {}

    [Serializable]
    public class SetManager : Command
    {
        public ActorRef Manager;
    }

    [Serializable]
    public class GetManager : Query<ActorRef>
    {}

    [Serializable]
    public class Greeting : Command
    {
        public ActorRef From;
        public string Text;
    }

    public class Employee : Actor
    {
        int level;
        ActorRef manager;

        protected override void Define()
        {
            On((Promote x)      => level = x.NewLevel);
            On((GetLevel x)     => level);
            On((SetManager x)   => manager = x.Manager);
            On((GetManager x)   => manager);
            On((Greeting x)     => Console.WriteLine("{0}: {1} said: {2}", Self, x.From, x.Text));
        }
    }
}
