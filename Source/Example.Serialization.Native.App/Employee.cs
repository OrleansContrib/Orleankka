using System;
using System.Linq;

using Orleankka;

namespace Example.Native.Serialization
{
    public class Employee : Actor
    {
        int level;
        ActorRef manager;

        protected override void Define()
        {
            On<Promote>(req => level = req.NewLevel);
            On<GetLevel, int>(req => level);
            On<SetManager>(req => manager = req.Manager);
            On<GetManager, ActorRef>(req => manager);
            On<Greeting>(req => Console.WriteLine("{0}: {1} said: {2}", Self, req.From, req.Text));
        }
    }
}
