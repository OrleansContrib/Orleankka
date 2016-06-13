using System;
using System.Collections.Generic;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    public class AzureClientConfigurator : IActorSystemConfigurator
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

        public AzureClientConfigurator Register(params ActorConfiguration[] configs)
        {
            client.Register(configs);
            return this;
        }

        IEnumerable<T> IActorSystemConfigurator.Hooks<T>() => client.Hooks<T>();
        void IActorSystemConfigurator.Hook<T>() => client.Hook(Activator.CreateInstance<T>());
        void IActorSystemConfigurator.Register(ActorConfiguration[] configs) => hooks.Add(Activator.CreateInstance<T>());

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
