using System;
using System.Collections.Generic;

using Orleankka;
using Orleankka.Meta;

namespace Example.Serialization.Native
{
    using System.Threading.Tasks;

    [Serializable]
    public class AddDirectReport : Command
    {
        public ActorRef Employee;
    }

    [Serializable]
    public class GetDirectReports : Query<IEnumerable<ActorRef>>
    {}

    [ActorTypeCode("manager")]
    public class Manager : Actor
    {
        readonly List<ActorRef> reports = new List<ActorRef>();

        async Task On(AddDirectReport x)
        {
            reports.Add(x.Employee);

            await x.Employee.Tell(new SetManager {Manager = Self});
            await x.Employee.Tell(new Greeting
            {
                From = Self,
                Text = "Welcome to my team!"
            });
        }

        IEnumerable<ActorRef> On(GetDirectReports x)
        {
            return reports.AsReadOnly();
        }
    }
}
