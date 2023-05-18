using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Orleans.Hosting;
using Orleans.Configuration;

using Orleankka.Cluster;

namespace Example
{
    class Program
    {
        const string DemoClusterId = "localhost-demo";
        const string DemoServiceId = "localhost-demo-service";
        const int LocalhostSiloPort = 11111;
        const int LocalhostGatewayPort = 30000;
        static readonly IPAddress LocalhostSiloAddress = IPAddress.Loopback;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Running demo. Booting cluster might take some time ...\n");

            var host = new HostBuilder()
                .UseOrleans(c => c
                    .Configure<ClusterOptions>(options => {
                        options.ClusterId = DemoClusterId;
                        options.ServiceId = DemoServiceId;
                    })
                    .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort))
                    .ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort))
                .UseOrleankka()
                .Build();

            await host.StartAsync();

            Console.WriteLine("Finished booting cluster...");
            Console.ReadLine();
        }
    }
}