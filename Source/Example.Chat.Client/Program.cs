using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Client;
using Orleankka.Typed;

using Orleans.Runtime.Configuration;

namespace Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Please wait until Chat Server has completed boot and then press enter.");
            Console.ReadLine();

            var config = new ClientConfiguration().LoadFromEmbeddedResource(typeof(Program), "Client.xml");

            var system = ActorSystem.Configure()
                .Client()
                .From(config)
                .Register(typeof (ChatServer).Assembly)
                .Done();

            var task = Task.Run(async () => await RunChatClient(system));
            task.Wait();
        }

        private static async Task RunChatClient(IActorSystem system)
        {
            //Get Chat room with name "Orleankka Chat Room"

            var chatRoom = system.TypedActorOf<ChatServer>("Orleankka Chat Room");

            Console.WriteLine("Enter your user name...");
            var userName = Console.ReadLine();

            using (var chatClient = new ChatClient(chatRoom, userName))
            {
                await chatClient.Start();

                Console.WriteLine("Connecting....");

                await chatClient.Join();

                Console.WriteLine("Connected!");

                while (true)
                {
                    var command = Console.ReadLine();

                    if (command == "quit")
                    {
                        await chatClient.Disconnect();
                        break;
                    }
                    await chatClient.Say(command);
                }
            }
        }
    }
}