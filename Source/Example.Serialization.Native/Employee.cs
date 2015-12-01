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
        long level;
        ActorRef manager;

        void On(Promote x)  => level = x.NewLevel;
        long On(GetLevel x) => level;

        void On(SetManager x)     => manager = x.Manager;
        ActorRef On(GetManager x) => manager;

        void On(Greeting x) => Console.WriteLine($"{x.From} said to {Self}: '{x.Text}'");
    }
}
