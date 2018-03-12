using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Checks
{
    [TestFixture]
    public class ClientRefFixture
    {
        [Test]
        public void Equatable_by_path()
        {
            var ref1 = new ClientRef("42");
            var ref2 = new ClientRef("42");
            
            Assert.True(ref1 == ref2);
            Assert.True(ref1.Equals(ref2));
        }
    }
}
