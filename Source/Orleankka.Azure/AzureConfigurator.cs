using System;
using System.Linq;

namespace Orleankka
{
    public interface IAzureConfigurator
    {}

    public static class AzureConfiguratorExtensions
    {
        public static IAzureConfigurator Azure(this IActorSystemConfigurator root)
        {
            return null;
        }
    }
}
