using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.ApplicationParts;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;

using Orleankka.Client;
using Orleans.Providers;

namespace Orleankka
{
    static class DemoExtensions
    {
        public static async Task<ISiloHost> Start(this ISiloHostBuilder builder)
        {
            var host = builder.Build();
            await host.StartAsync();
            return host;
        }

        public static async Task<IClusterClient> Connect(this ISiloHost host, Action<ClientConfiguration> configure = null)
        {
            var config = ClientConfiguration.LocalhostSilo();
            configure?.Invoke(config);

            var cluster = host.Services.GetRequiredService<ClusterConfiguration>();
            if (cluster.Globals.ProviderConfigurations.TryGetValue(ProviderCategoryConfiguration.STREAM_PROVIDER_CATEGORY_NAME, out ProviderCategoryConfiguration pcc))
            {
                foreach (var each in pcc.Providers)
                    config.RegisterStreamProvider(each.Value.Type, each.Key, each.Value.Properties);
            }

            var client = new ClientBuilder()
                .UseConfiguration(config)
                .ConfigureApplicationParts(x =>
                {
                    var apm = host.Services.GetRequiredService<ApplicationPartManager>();
                    foreach (var part in apm.ApplicationParts.OfType<AssemblyPart>())
                        x.AddApplicationPart(part.Assembly);
                })
                .ConfigureOrleankka()
                .Build();

            await client.Connect();
            return client;
        }

        public static ClusterConfiguration UseSerializer<T>(this ClusterConfiguration cfg)
        {
            cfg.Globals.SerializationProviders.Add(typeof(T).GetTypeInfo());
            return cfg;
        }
        
        public static ClientConfiguration UseSerializer<T>(this ClientConfiguration cfg)
        {
            cfg.SerializationProviders.Add(typeof(T).GetTypeInfo());
            return cfg;
        }
    }

    public abstract class BootstrapProvider : IBootstrapProvider
    {
        public string Name { get; private set; }

        Task IProvider.Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;
            return Init(providerRuntime, config);
        }

        protected abstract Task Init(IProviderRuntime runtime, IProviderConfiguration config);

        Task IProvider.Close() => Task.CompletedTask;
    }
}