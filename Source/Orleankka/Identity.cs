using System;
using System.Linq;

using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    static class Identity
    {
        public static string Of(Actor actor)
        {
            string id;
            actor.GetPrimaryKey(out id);
            return id;
        }        
        
        public static string Of(IActorObserver observer)
        {
            return ((GrainReference)observer).ToKeyString();
        }
    }
}
