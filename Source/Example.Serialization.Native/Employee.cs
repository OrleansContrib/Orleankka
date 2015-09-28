using System;

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

    [ActorTypeCode("employee")]
    public class Employee : Actor
    {
        int level;
        ActorRef manager;

        void On(Promote x)
        {
            level = x.NewLevel;
        }

        long On(GetLevel x)
        {
            return level;
        }

        void On(SetManager x)
        {
            manager = x.Manager;
        }

        ActorRef On(GetManager x)
        {
            return manager;
        }

        void On(Greeting x)
        {
            Console.WriteLine("{0}: {1} said: {2}", Self, x.From, x.Text);
        }
    }
}
