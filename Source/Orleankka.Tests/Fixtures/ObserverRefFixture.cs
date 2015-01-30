using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Fixtures
{
    [TestFixture]
    public class ObserverRefFixture
    {
        [Test]
        public void Observer_ref_is_equatable_by_path()
        {
            var ref1 = new ObserverRef(ObserverPath.From("42"), null);
            var ref2 = new ObserverRef(ObserverPath.From("42"), null);
            
            Assert.True(ref1 == ref2);
            Assert.True(ref1.Equals(ref2));
        }
    }
}
