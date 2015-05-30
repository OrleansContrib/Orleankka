using System;
using System.Linq;

using Orleankka;

namespace Example
{
    public class DIActor : UntypedActor
    {
        readonly ISomeService service;

        DIActor()
        {}

        public DIActor(ISomeService service)
        {
            this.service = service;
        }

        public void Handle(string msg)
        {
            service.SayHello(msg, Self);
        }
    }
}
