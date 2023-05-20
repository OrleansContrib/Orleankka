using System;
using System.Net;
using System.Threading.Tasks;

using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Configuration;

using Orleankka.Client;

namespace Example
{
    using Microsoft.Extensions.Hosting;

    class Program
    {
        const string DemoClusterId = "localhost-demo";
        const string DemoServiceId = "localhost-demo-service";
        const int LocalhostGatewayPort = 30000;
        static readonly IPAddress LocalhostSiloAddress = IPAddress.Loopback;
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Please wait until Chat Server has completed boot and then press enter.");
            Console.ReadLine();
            
            Console.WriteLine("Connecting to server ...");

            var system = await Connect(retries: 2);            

            Console.WriteLine("Enter your user name...");
            var userName = Console.ReadLine();

            const string room = "Orleankka";

            var client = new ChatClient(system, userName, room);
            await client.Join();

            Console.WriteLine("Enter your messages or `quit` to terminate");

            while (true)
            {
                var message = Console.ReadLine();

                if (message == "quit")
                {
                    await client.Leave();
                    break;
                }

                await client.Say(message);
            }
        }

        static async Task<IClientActorSystem> Connect(int retries = 0, TimeSpan? retryTimeout = null)
        {
            if (retryTimeout == null)
                retryTimeout = TimeSpan.FromSeconds(5);

            if (retries < 0)
                throw new ArgumentOutOfRangeException(nameof(retries), 
                    "retries should be greater than or equal to 0");

            while (true)
            {
                try
                {
                    var host = new HostBuilder()
                        .UseOrleansClient(c => c.UseLocalhostClustering())
                        .UseOrleankka()
                        .Build();

                    await host.StartAsync();
                    Console.WriteLine("Connected!");
                    
                    return host.ActorSystem();
                }
                catch (Exception ex)
                {
                    if (retries-- == 0)
                    {
                        Console.WriteLine("Can't connect to cluster. Max retries reached.");
                        throw;
                    }

                    Console.WriteLine($"Can't connect to cluster: '{ex.Message}'. Trying again in {(int)retryTimeout.Value.TotalSeconds} seconds ...");
                    await Task.Delay(retryTimeout.Value);
                }
            }
        }
    }
}