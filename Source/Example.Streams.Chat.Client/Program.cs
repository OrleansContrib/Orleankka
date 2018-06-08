using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleankka;
using Orleankka.Client;

using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Example
{
    class Program
    {
        public static event Action OnClusterConnectionLost = () => {};

        static void Main(string[] args)
        {
            Console.WriteLine("Please wait until Chat Server has completed boot and then press enter.");
            Console.ReadLine();

            var system = ActorSystem.Configure()
                .Client()
                .Builder(b => b.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "test";
                        options.ServiceId = "test";
                    })
                    .UseLocalhostClustering()
                    .AddSimpleMessageStreamProvider("sms"))
                .Assemblies(typeof(IChatUser).Assembly)
                .Services(x => x
                    .AddSingleton<ConnectionToClusterLostHandler>((s, e) => OnClusterConnectionLost()))
                .Done();

            var task = Task.Run(async () => await RunChatClient(system));
            task.Wait();
        }

        static async Task RunChatClient(ClientActorSystem system)
        {
            const string room = "Orleankka";

            Console.WriteLine("Connecting to server ...");
            await system.Connect(retries: 2);

            Console.WriteLine("Enter your user name...");
            var userName = Console.ReadLine();
            
            var client = new ChatClient(system, userName, room);
            OnClusterConnectionLost += ()=> client.Resubscribe().Wait();

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
    }
}