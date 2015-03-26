using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Testing
{
    [TestFixture]
    public abstract class ActorSystemScenario
    {
        protected IActorSystem system;

        [SetUp]
        public void SetUp()
        {
            system = TestActorSystem.Instance;
        }
    }
}
