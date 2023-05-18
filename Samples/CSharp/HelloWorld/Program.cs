using System;
using System.Threading.Tasks;
using System.Net;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;

using Orleankka;
using Orleankka.Cluster;

using static System.Console;

namespace Demo
{
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

            var host = new HostBuilder()
                .UseOrleans(c => c
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = DemoClusterId;
                        options.ServiceId = DemoServiceId;
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
                    .ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort))
                .UseOrleankka()
                .Build();

            await host.StartAsync();

            var greeter = host.ActorSystem().ActorOf<IGreeter>("id");
            await greeter.Tell(new Greet {Who = "world"});

            Write("\n\nPress any key to terminate ...");
            ReadKey(true);
        }
    }
}
