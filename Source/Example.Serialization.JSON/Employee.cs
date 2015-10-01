using System;

using Orleankka;
using Orleankka.Meta;

namespace Example.Serialization.JSON
{
    class GetLevel : Query<long> {}
    class Promote  : Command
    {
        public long NewLevel;
    }

    class GetManager : Query<ActorRef> {}
    class SetManager : Command
    {
        public ActorRef Manager;
    }

    class Greeting : Command
    {
        public ActorRef From;
        public string Text;
    }

    [ActorTypeCode("employee")]
    class Employee : Actor
    {
        long level;
        ActorRef manager;

        void On(Promote x)  		=> level = x.NewLevel;
        long On(GetLevel x) 		=> level;

        void On(SetManager x)     	=> manager = x.Manager;
        ActorRef On(GetManager x) 	=> manager;

        void On(Greeting x) 		=> Console.WriteLine($"{x.From} said to {Self}: '{x.Text}'");
    }
}
