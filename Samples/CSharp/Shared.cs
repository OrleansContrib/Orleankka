using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Runtime;

namespace Orleankka
{
    static class DemoExtensions
    {
        const string DemoClusterId = "localhost-demo";
        const string DemoServiceId = "localhost-demo-service";

        const int LocalhostSiloPort = 11111;
        const int LocalhostGatewayPort = 30000;
        static readonly IPAddress LocalhostSiloAddress = IPAddress.Loopback;

        static ISiloBuilder ConfigureDemoClustering(this ISiloBuilder builder) => builder
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = DemoClusterId;
                options.ServiceId = DemoServiceId;
            })
            .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort))
            .ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort);

        static IClientBuilder ConfigureDemoClustering(this IClientBuilder builder) => builder
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = DemoClusterId;
                options.ServiceId = DemoServiceId;
            })
            .UseStaticClustering(options => options.Gateways.Add(new IPEndPoint(LocalhostSiloAddress, LocalhostGatewayPort).ToGatewayUri()));

        public static ISiloBuilder ConfigureServer(this ISiloBuilder builder)
        {
            return builder
                .ConfigureDemoClustering()
                .AddMemoryGrainStorageAsDefault()
                .AddMemoryGrainStorage("PubSubStore")
                .AddMemoryStreams("sms")
                .UseInMemoryReminderService();
        }

        /*
        public static async Task<IHost> Start(this ISiloBuilder builder)
        {
            var host = builder
                .ConfigureDemoClustering()
                .AddMemoryGrainStorageAsDefault()
                .AddMemoryGrainStorage("PubSubStore")
                .AddMemoryStreams("sms")
                .UseInMemoryReminderService()
                .Build();

            await host.StartAsync();
            return host;
        }

        public static async Task<IClusterClient> Connect(this ISiloBuilder host, Action<IClientBuilder> configure = null)
        {
            var builder = new ClientBuilder()
                .ConfigureDemoClustering()
                .AddSimpleMessageStreamProvider("sms")
                .UseOrleankka();

            configure?.Invoke(builder);
            var client = builder.Build();

            await client.Connect();
            return client;
        }
    */
    }
}