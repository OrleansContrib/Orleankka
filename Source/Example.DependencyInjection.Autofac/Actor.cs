using System;
using System.Linq;

using Orleankka;

namespace Example
{
    public class DIActor : Actor
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
