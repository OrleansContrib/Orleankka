using Orleankka;

namespace Example
{
    public class DIActor : Actor
    {
        readonly ISomeService service;

        DIActor() 
        {}

        public DIActor(string id, IActorRuntime runtime, Dispatcher dispatcher, ISomeService service) 
		    : base(id, runtime, dispatcher)
        {
            this.service = service;
        }

        public void Handle(string msg)
        {
            service.SayHello(msg, Self);
        }
    }
}
