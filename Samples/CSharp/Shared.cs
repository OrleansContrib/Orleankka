using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Orleans.Hosting;
using Orleans.Configuration;

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

        public static async Task<IHost> StartServer(this IHostBuilder builder)
        {
            return await builder
                .UseOrleans(c => c
                    .ConfigureDemoClustering()
                    .AddMemoryGrainStorageAsDefault()
                    .AddMemoryGrainStorage("PubSubStore")
                    .AddMemoryStreams("sms")
                    .UseInMemoryReminderService())
                .StartAsync();
        }
    }
}