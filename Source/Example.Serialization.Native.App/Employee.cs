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
            On((Promote x)      => level = x.NewLevel);
            On((GetLevel x)     => level);
            On((SetManager x)   => manager = x.Manager);
            On((GetManager x)   => manager);
            On((Greeting x)     => Console.WriteLine("{0}: {1} said: {2}", Self, x.From, x.Text));
        }
    }
}
