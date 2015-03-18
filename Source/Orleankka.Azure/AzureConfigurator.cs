using System;
using System.Linq;

namespace Orleankka
{
    using Utility;

    public class AzureConfigurator
    {
        internal readonly IActorSystemConfigurator Configurator;

        internal AzureConfigurator(IActorSystemConfigurator configurator)
        {
            Configurator = configurator;
        }
    }

    public static class AzureConfiguratorExtensions
    {
        public static AzureConfigurator Azure(this IActorSystemConfigurator configurator)
        {
            Requires.NotNull(configurator, "configurator");
            return new AzureConfigurator(configurator);
        }
    }
}
