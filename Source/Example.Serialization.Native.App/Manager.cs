using System;
using System.Collections.Generic;
using System.Linq;

using Orleankka;

namespace Example.Native.Serialization
{
    public class Manager : Actor
    {
        readonly List<ActorRef> reports = new List<ActorRef>();

        protected override void Define()
        {
            On<AddDirectReport>(async req =>
            {
                reports.Add(req.Employee);
                await req.Employee.Tell(new SetManager {Manager = Self});                
                await req.Employee.Tell(new Greeting {From = Self, Text = "Welcome to my team!"});                
            });

            On<GetDirectReports, IEnumerable<ActorRef>>(req => reports);
        }
    }
}
