using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    public abstract class Actor : Grain, IActor
    {
        string id;
        ActorPath path;
        readonly IActorSystem system;

        protected Actor()
        {
            system = ActorSystem.Instance;
        }
        
        protected Actor(string id, IActorSystem system = null)
        {
            Requires.NotNullOrWhitespace(id, "id");

            this.id = id;
            this.system = system;
        }

        public ActorPath Path
        {
            get { return (path ?? (path = ActorPath.Map(GetType(), Id))); }
        }

        public string Id
        {
            get { return (id ?? (id = Identity.Of(this))); }
        }

        public IActorRef Self()
        {
            return system.ActorOf(Path);
        }

        public virtual Task OnTell(object message)
        {
            throw NotImplemented("OnTell");
        }

        public virtual Task<object> OnAsk(object message)
        {
            throw NotImplemented("OnAsk");
        }

        NotImplementedException NotImplemented(string method)
        {
            return new NotImplementedException(string.Format(
                "Override {0}() method in class {1} to implement corresponding behavior", 
                method, GetType())
            );
        }

        public Notification Notification(object message)
        {
            return new Notification(Path, message);
        }
    }
}
