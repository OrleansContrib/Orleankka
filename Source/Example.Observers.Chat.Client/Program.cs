using System;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Client;

using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Example
{
    class Program
    {
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
                .Assemblies(typeof(Join).Assembly)
                .Done();

            var task = Task.Run(async () => await RunChatClient(system));
            task.Wait();
        }

        private static async Task RunChatClient(ClientActorSystem system)
        {
            const string room = "Orleankka";

            Console.WriteLine("Connecting to server ...");
            await system.Connect(retries: 5);

            Console.WriteLine("Enter your user name...");
            var userName = Console.ReadLine();

            var client = new ChatClient(system, userName, room);
            await client.Join();

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
    }
}