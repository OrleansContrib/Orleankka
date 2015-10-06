using System.Threading.Tasks;
using System.Collections.Generic;

using Orleankka;
using Orleankka.Meta;

namespace Example.Serialization.JSON
{
    class GetDirectReports : Query<Manager, IEnumerable<ActorRef<Employee>>> {}
    class AddDirectReport  : Command<Manager>
    {
        public ActorRef<Employee> Employee;
    }

    [ActorTypeCode("manager")]
    class Manager : Actor
    {
        readonly List<ActorRef<Employee>> reports = new List<ActorRef<Employee>>();

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

        IEnumerable<ActorRef<Employee>> On(GetDirectReports x) => reports.AsReadOnly();
    }
}
