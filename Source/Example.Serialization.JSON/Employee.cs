using System;

using Orleankka;
using Orleankka.Meta;

namespace Example.Serialization.JSON
{
    class GetLevel : Query<Employee, long> {}
    class Promote  : Command<Employee>
    {
        public long NewLevel;
    }

    class GetManager : Query<Employee, ActorRef> {}
    class SetManager : Command<Employee>
    {
        public ActorRef<Manager> Manager;
    }

    class Greeting : Command<Employee>
    {
        public ActorRef<Manager> From;
        public string Text;
    }

    [ActorTypeCode("employee")]
    class Employee : Actor
    {
        long level;
        ActorRef<Manager> manager;

        void On(Promote x)  => level = x.NewLevel;
        long On(GetLevel x) => level;

        void On(SetManager x) => manager = x.Manager;
        ActorRef<Manager> On(GetManager x) => manager;

        void On(Greeting x) => Console.WriteLine($"{x.From} said to {Self}: '{x.Text}'");
    }
}
