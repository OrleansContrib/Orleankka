using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    using Core;
    using Utility;

    public class AzureClientConfigurator
    {
        readonly AzureConfigurator azure;
        readonly ClientConfigurator client;

        internal AzureClientConfigurator(AzureConfigurator azure)
        {
            this.azure = azure;
            client = new ClientConfigurator(azure.Configurator);
        }

        public ClientConfiguration Configuration
        {
            get { return client.Configuration; }
        }

        public AzureClientConfigurator From(ClientConfiguration config)
        {
            client.From(config);
            return this;
        }

        public AzureClientConfigurator Serializer<T>(Dictionary<string, string> properties = null) where T : IMessageSerializer
        {
            client.Serializer<T>(properties);
            return this;
        }

        public AzureClientConfigurator Register(params Assembly[] assemblies)
        {
            client.Register(assemblies);
            return this;
        }

        public IActorSystem Done()
        {
            var system = new AzureClientActorSystem(azure.Configurator);
            client.Configure(system);

            AzureClientActorSystem.Initialize(Configuration);
            return system;
        }
    }

    public static class ClientConfiguratorExtensions
    {
        public static AzureClientConfigurator Client(this AzureConfigurator configurator)
        {
            Requires.NotNull(configurator, "configurator");
            return new AzureClientConfigurator(configurator);
        }
    }
}
