﻿using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Storage;

using Orleankka.Cluster;

namespace Example
{
    using ClusterOptions = Orleans.Configuration.ClusterOptions;

    class Program
    {
        const string DemoClusterId = "localhost-demo";
        const string DemoServiceId = "localhsot-demo-service";

        const int LocalhostSiloPort = 11111;
        const int LocalhostGatewayPort = 30000;
        static readonly IPAddress LocalhostSiloAddress = IPAddress.Loopback;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Running demo. Booting cluster might take some time ...\n");

            var host = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = DemoClusterId;
                    options.ServiceId = DemoServiceId;
                })
                .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort))
                .ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort)
                .AddMemoryGrainStorage("PubSubStore")
                .AddSimpleMessageStreamProvider("sms")
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .AddApplicationPart(typeof(Join).Assembly)
                    .AddApplicationPart(typeof(MemoryGrainStorage).Assembly)
                    .WithCodeGeneration())
                .UseOrleankka()
                .Build();

            await host.StartAsync();

            Console.WriteLine("Finished booting cluster...");
            Console.ReadLine();
        }
    }
}