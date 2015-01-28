using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka
{
    [TestFixture]
    public class TodoFixture
    {
        [Test]
        public void Yay()
        {
            /* 
                            
                                - Check DynamicActorObserver idempotence, since it's a wrapper, it should be as transparent as possible
                                   It should exhibit the same behaviour as underlying instance. Orleans will give the same reference each time.
                                
                                - Check Reminders with DynamicActors. Introduce explicit assembly registration. Enumerate types and use type name as typecode.
                                  Make sure Orleans' TypeCodeOverride is utilized.
                                
                               - Hardcode ClientObserbvable type, when serialized to just: client, co, obs - choose smth short and up to the point
             
                               - Introduce flavors: permutations of all possible host configurations. Check it by creating actors with various flavors. 
                                 You can dynamically generate assembly with all possible permutations and then register it. And then simply check the type of the host returned.
             
                              - There is a single attribute that cannot be placed on class: UnorderedAttribute. Make your own, such as DelieveryOrderAgnostic
                                 Contribute to Orleans by making it placeable on a class, if that will be ok for the owners.
             
                            */
        }
    }
}
