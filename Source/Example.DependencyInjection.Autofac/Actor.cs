using System;
using System.Linq;

using Orleankka;
using Orleankka.CSharp;

namespace Example
{
    public class DIActor : Actor
    {
        readonly ISomeService service;

        DIActor() 
        {}

        public DIActor(string id, IActorRuntime runtime, ISomeService service) : base(id, runtime)
        {
            this.service = service;
        }

        public void Handle(string msg)
        {
            service.SayHello(msg, Self);
        }
    }
}
