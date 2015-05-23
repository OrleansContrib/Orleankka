using System;
using System.Threading.Tasks;
using Orleankka;
using Orleankka.Client;
using Orleans.Runtime.Configuration;
using Server;

namespace CSharp.Demo.Chat.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Please wait until Chat Server has completed boot and then press enter.");
            Console.ReadLine();

            var config = new ClientConfiguration().LoadFromEmbeddedResource(typeof (Program),
                "Client.xml");

            var system = ActorSystem.Configure()
                .Client()
                .From(config)
                .Register(typeof (ChatServer).Assembly)
                .Done();

            var client = Observer.Create().Result;

            var server = system.ActorOf<ChatServer>("server");

            Console.WriteLine("Enter your user name...");
            var userName = Console.ReadLine();

            var chatClient = new ChatClient(server, client.Ref, userName);

            var observer = client.Subscribe(o => chatClient.Handle((dynamic) o));

            var task = Task.Run(async () => await RunChatClient(chatClient));
            task.Wait();
        }

        private static async Task RunChatClient(ChatClient chatClient)
        {
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