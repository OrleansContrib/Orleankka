using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Client;
using Orleans.Runtime.Configuration;

namespace Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Please wait until Chat Server has completed boot and then press enter.");
            Console.ReadLine();

            var config = new ClientConfiguration()
                .LoadFromEmbeddedResource(typeof(Program), "Client.xml");

            var system = ActorSystem.Configure()
                .Client()
                .From(config)
                .Register(typeof(ChatRoom).Assembly)
                .Done();

            var task = Task.Run(async () => await RunChatClient(system));
            task.Wait();
        }

        private static async Task RunChatClient(IActorSystem system)
        {
            const string room = "Orleankka";

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