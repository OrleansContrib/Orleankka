using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka
{
    [TestFixture]
    public class TodoFixture
    {
        [Test, Ignore]
        public void TypeCode()
        {
            // - Provide TypeCodeAttribute to be able to override default type name strategy
        }

        [Test, Ignore]
        public void Flavors()
        {
            // - Introduce flavors: permutations of all possible host configurations. Check it by creating actors with various flavors. 
            // You can dynamically generate assembly with all possible permutations and then register it. And then simply check the type of the host returned. 

            // - There is a single attribute that cannot be placed on class: UnorderedAttribute. Make your own, such as DelieveryOrderAgnostic
            // Contribute to Orleans by making it placeable on a class, if that will be ok for the owners.
        }
    }
}
