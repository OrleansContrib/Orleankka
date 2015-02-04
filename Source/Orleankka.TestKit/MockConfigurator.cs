using System;
using System.Linq;

namespace Orleankka.TestKit
{
    public class MockConfigurator
    {
        readonly IActorSystemConfigurator configurator;

        public MockConfigurator(IActorSystemConfigurator configurator)
        {
            this.configurator = configurator;
        }
    }

    public static class MockConfiguratorExtensions
    {
        public static MockConfigurator Mock(this ActorSystemConfigurator configurator)
        {
            Requires.NotNull(configurator, "configurator");
            return new MockConfigurator(configurator);
        }
    }
}