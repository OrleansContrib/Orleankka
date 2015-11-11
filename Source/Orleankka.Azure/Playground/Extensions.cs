using System;
using System.Collections.Generic;

using Orleans.Storage;

namespace Orleankka.Playground
{
    public static class PlaygroundConfiguratorExtensions
    {
        public static PlaygroundConfigurator UseAzureTablePubSubStore(this PlaygroundConfigurator configurator)
        {
            var properties = new Dictionary<string, string>()
            {
                {"DataConnectionString", "UseDevelopmentStorage=true"},
                {"UseJsonFormat", "true"},
                {"TableName", "PubSubData"}
            };

            configurator.RegisterPubSubStorageProvider<AzureTableStorage>(properties);
            return configurator;
        }
    }
}