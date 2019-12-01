using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Orleankka;
using Orleankka.Client;
using Orleankka.Cluster;

using Orleans;
using Orleans.Hosting;

using static System.Console;

namespace Demo
{
    using System.Net;

    using Orleans.Configuration;
    using Orleans.Runtime;

    [Serializable]
    public class Greet
    {
        public string Who { get; set; }
    }

    public interface IGreeter : IActorGrain, IGrainWithStringKey {}

    public class Greeter : DispatchActorGrain, IGreeter
    {
        void On(Greet msg) => WriteLine($"Hello, {msg.Who}!");
    }

    public static class Program
    {
        const string DemoClusterId = "localhost-demo";
        const string DemoServiceId = "localhost-demo-service";

        const int LocalhostSiloPort = 11111;
        const int LocalhostGatewayPort = 30000;
        static readonly IPAddress LocalhostSiloAddress = IPAddress.Loopback;

        public static async Task Main()
        {
            WriteLine("Running example. Booting cluster might take some time ...\n");

            var host = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = DemoClusterId;
                    options.ServiceId = DemoServiceId;
                })
                .Configure<SchedulingOptions>(options =>
                {
                    options.AllowCallChainReentrancy = false;
                })
                .Configure<SiloMessagingOptions>(options =>
                {
                    options.ResponseTimeout = TimeSpan.FromSeconds(5);
                    options.ResponseTimeoutWithDebugger = TimeSpan.FromSeconds(5);
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddConsole();
                })
                .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort))
                .ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort)
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .UseOrleankka()
                .Build();

            await host.StartAsync();

            var client = new ClientBuilder()
                .Configure<ClusterOptions>(options => {
                    options.ClusterId = DemoClusterId;
                    options.ServiceId = DemoServiceId;
                })
                .UseStaticClustering(options => options.Gateways.Add(new IPEndPoint(LocalhostSiloAddress, LocalhostGatewayPort).ToGatewayUri()))
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .UseOrleankka()
                .Build();

            await client.Connect();

            var greeter = client.ActorSystem().ActorOf<IGreeter>("id");
            await greeter.Tell(new Greet {Who = "world"});

            Write("\n\nPress any key to terminate ...");
            ReadKey(true);
        }
    }
}
