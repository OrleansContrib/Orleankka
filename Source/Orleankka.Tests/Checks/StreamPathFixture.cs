using NUnit.Framework;

namespace Orleankka.Checks
{
    [TestFixture]
    public class StreamPathFixture
    {
        [Test]
        public void Can_be_constructed_and_serialized_without_stream_provider_registration()
        {
            var path = StreamPath.From("sms", "42");
            Assert.AreEqual("sms:42", path.Serialize());
        }
    }
}
