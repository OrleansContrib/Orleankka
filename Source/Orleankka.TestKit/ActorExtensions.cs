using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    public static class ActorExtensions
    {
        public static Task OnActivate(this Actor actor)
        {
            return actor.OnActivate();
        }

        public static Task OnDeactivate(this Actor actor)
        {
            return actor.OnDeactivate() ;
        }

        public static Task<object> OnReceive(this Actor actor, object message)
        {
            return actor.OnReceive(message);
        }

        public static Task OnReminder(this Actor actor, string id)
        {
            return actor.OnReminder(id);
        }

        public static void Define(this Actor actor)
        {
            actor.Define();
        }
    }
}
