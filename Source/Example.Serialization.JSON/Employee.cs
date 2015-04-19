using System;
using System.Linq;

using Orleankka;
using Orleankka.Meta;

namespace Example.Serialization.JSON
{
    public class Promote
    {
        public long NewLevel;
    }

    public class GetLevel : Query<long>
    {}

    public class SetManager : Command
    {
        public ActorRef Manager;
    }

    public class GetManager : Query<ActorRef>
    {}

    public class Greeting : Command
    {
        public ActorRef From;
        public string Text;
    }

    public class Employee : Actor
    {
        long level;
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
