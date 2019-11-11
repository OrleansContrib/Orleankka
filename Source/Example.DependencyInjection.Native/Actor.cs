using Orleankka;

namespace Example
{
    public interface IDIActor : IActor {}

    public class DIActor : Actor, IDIActor
    {
        readonly ISomeService service;

        public DIActor(ISomeService service) 
		    : base("id", null)
        {
            this.service = service;
        }

        public void Handle(string msg)
        {
            service.SayHello(msg, Self);
        }
    }
}
