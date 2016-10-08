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
        public void Reentrancy_is_full_or_partial()
        {
            var cfg = new ActorConfiguration("id");

            cfg.Reentrant = true;
            Assert.Throws<InvalidOperationException>(() => cfg.InterleavePredicate = x => false);

            cfg.Reentrant = false;
            Assert.DoesNotThrow(() => cfg.InterleavePredicate = x => false);

            cfg.InterleavePredicate = x => false;
            Assert.Throws<InvalidOperationException>(() => cfg.Reentrant = true);

            cfg.InterleavePredicate = null;
            Assert.DoesNotThrow(() => cfg.Reentrant = true);
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
