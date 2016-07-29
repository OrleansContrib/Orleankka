using System;

using NUnit.Framework;

namespace Orleankka.Checks
{
    [TestFixture]
    public class ActorConfigurationFixture
    {
        [Test]
        public void Requires_id()
        {
            Assert.Throws<ArgumentException>(()=> new ActorConfiguration(""));
            Assert.Throws<ArgumentNullException>(()=> new ActorConfiguration(null));
        }

        [Test]
        public void Reentrancy_options()
        {
            var cfg = new ActorConfiguration("id");
            Assert.Throws<ArgumentNullException>(() => cfg.IsReentrant = null);
        }

        [Test]
        public void Keep_alive_options()
        {
            var cfg = new ActorConfiguration("id");
            Assert.Throws<ArgumentException>(() => cfg.KeepAliveTimeout = TimeSpan.FromSeconds(59), 
                "Keep alive should be greater than zero");
        }
    }
}
