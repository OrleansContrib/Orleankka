using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Hosting;

using Orleankka.Client;

namespace Example
{
    class Program
    {
        public static event Action OnClusterConnectionLost = () => {};

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
            OnClusterConnectionLost += async ()=> await client.Resubscribe();

            await client.Join();

            while (true)
            {
                var message = Console.ReadLine();

                if (message == "quit")
                {
                    await client.Leave();
                    break;
                }

                if (message == "reconnect")
                {
                    await client.Resubscribe();
                    continue;
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
                        .UseOrleansClient(c => c
                            .UseLocalhostClustering()
                            .AddMemoryStreams("sms")
                        .ConfigureServices(x => x
                            .AddSingleton<ConnectionToClusterLostHandler>((s, e) => OnClusterConnectionLost())))
                        .UseOrleankka()
                        .Build();

                    await host.StartAsync();
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