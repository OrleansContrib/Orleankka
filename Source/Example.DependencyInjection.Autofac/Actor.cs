using Orleankka;
using Orleankka.CSharp;

namespace Example
{
    public class DIActor : Actor
    {
        readonly ISomeService service;

        // parameterless ctor is still required
        DIActor() 
        {}

        public DIActor(IActorContext context, Dispatcher dispatcher, ISomeService service) 
            : base(context, dispatcher)
        {
            this.service = service;
        }

        public void Handle(string msg)
        {
            service.SayHello(msg, Self);
        }
    }
}
