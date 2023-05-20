using NUnit.Framework;

namespace Orleankka.Checks
{
    [TestFixture]
    public class StreamRefFixture
    {
        [Test]
        public void Equatable_by_path()
        {
            var path = StreamPath.From("sms", "42");

            var ref1 = new StreamRef<string>(path, null, null);
            var ref2 = new StreamRef<string>(path, null, null);
            
            Assert.True(ref1 == ref2);
            Assert.True(ref1.Equals(ref2));
        }
    }
}
