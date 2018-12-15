using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.ApplicationParts;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

using Orleankka.Client;

namespace Orleankka
{
    static class DemoExtensions
    {
        const string DemoClusterId = "localhost-demo";
        const int LocalhostSiloPort = 11111;
        const int LocalhostGatewayPort = 30000;
        static readonly IPAddress LocalhostSiloAddress = IPAddress.Loopback;

        static ISiloHostBuilder ConfigureDemoClustering(this ISiloHostBuilder builder) => builder
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = DemoClusterId;
                options.ServiceId = DemoClusterId;
            })
            .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort))
            .ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort);

        static IClientBuilder ConfigureDemoClustering(this IClientBuilder builder) => builder
            .Configure<ClusterOptions>(options =>
            {
                options.ServiceId = DemoClusterId;
                options.ClusterId = DemoClusterId;
            })
            .UseStaticClustering(options => options.Gateways.Add(new IPEndPoint(LocalhostSiloAddress, LocalhostGatewayPort).ToGatewayUri()));

        public static async Task<ISiloHost> Start(this ISiloHostBuilder builder)
        {
            var host = builder
                .ConfigureDemoClustering()
                .AddMemoryGrainStorageAsDefault()
                .AddMemoryGrainStorage("PubSubStore")
                .AddSimpleMessageStreamProvider("sms")
                .ConfigureApplicationParts(x => x.AddApplicationPart(typeof(MemoryGrainStorage).Assembly))
                .UseInMemoryReminderService()
                .Build();

            await host.StartAsync();
            return host;
        }

        public static async Task<IClusterClient> Connect(this ISiloHost host, Action<IClientBuilder> configure = null)
        {
            var builder = new ClientBuilder()
                .ConfigureDemoClustering()
                .AddSimpleMessageStreamProvider("sms")
                .ConfigureApplicationParts(x =>
                {
                    var apm = host.Services.GetRequiredService<IApplicationPartManager>();
                    foreach (var part in apm.ApplicationParts.OfType<AssemblyPart>())
                        x.AddApplicationPart(part.Assembly);
                })
                .UseOrleankka();

            configure?.Invoke(builder);
            var client = builder.Build();

            await client.Connect();
            return client;
        }

        public static ISiloHostBuilder UseSerializer<T>(this ISiloHostBuilder builder)
        {
            return builder.ConfigureServices(services => services.Configure<SerializationProviderOptions>(options =>
            {
                options.SerializationProviders.Add(typeof(T).GetTypeInfo());
            }));
        }
        
        public static IClientBuilder UseSerializer<T>(this IClientBuilder builder)
        {
            return builder.ConfigureServices(services => services.Configure<SerializationProviderOptions>(options =>
            {
                options.SerializationProviders.Add(typeof(T).GetTypeInfo());
            }));
        }
    }
}