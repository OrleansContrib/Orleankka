using System;
using System.Collections.Generic;
using System.Linq;

using Orleankka;
using Orleankka.Meta;

namespace Example.Serialization.JSON
{
    [Serializable]
    public class AddDirectReport : Command
    {
        public ActorRef Employee;
    }

    [Serializable]
    public class GetDirectReports : Query<IEnumerable<ActorRef>>
    {}

    public class Manager : Actor
    {
        readonly List<ActorRef> reports = new List<ActorRef>();

        protected override void Define()
        {
            On<AddDirectReport>(async x =>
            {
                reports.Add(x.Employee);
                
                await x.Employee.Tell(new SetManager {Manager = Self});                
                await x.Employee.Tell(new Greeting
                {
                    From = Self, 
                    Text = "Welcome to my team!"
                });                
            });

            On((GetDirectReports x) => reports.ToList());
        }
    }
}
