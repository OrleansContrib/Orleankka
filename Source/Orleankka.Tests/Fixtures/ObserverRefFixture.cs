using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Fixtures
{
    [TestFixture]
    public class ObserverRefFixture
    {
        [Test]
        public void Equatable_by_path()
        {
            var ref1 = new ObserverRef("42");
            var ref2 = new ObserverRef("42");
            
            Assert.True(ref1 == ref2);
            Assert.True(ref1.Equals(ref2));
        }
    }
}
