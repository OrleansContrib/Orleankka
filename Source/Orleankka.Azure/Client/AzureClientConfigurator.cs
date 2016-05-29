using System.Collections.Generic;
using System.Reflection;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    using Core;

    public class AzureClientConfigurator
    {
        readonly ClientConfigurator client;

        internal AzureClientConfigurator()
        {
            client = new ClientConfigurator();
        }

        public AzureClientConfigurator From(ClientConfiguration config)
        {
            client.From(config);
            return this;
        }

        public AzureClientConfigurator Register<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProvider
        {
            client.Register<T>(name, properties);
            return this;
        }

        public AzureClientConfigurator Register(params Assembly[] assemblies)
        {
            client.Register(assemblies);
            return this;
        }

        public IActorSystem Done()
        {
            var system = new AzureClientActorSystem(client);
            client.Configure();

            AzureClientActorSystem.Initialize(client.Configuration);
            return system;
        }
    }

    public static class ClientConfiguratorExtensions
    {
        public static AzureClientConfigurator Client(this IAzureConfigurator root)
        {
            return new AzureClientConfigurator();
        }
    }
}
