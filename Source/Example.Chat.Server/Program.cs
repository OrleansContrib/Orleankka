using System;
using System.Reflection;
using Orleankka;
using Orleankka.Cluster;
using Orleans.Runtime.Configuration;

namespace Example.Chat.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Running demo. Booting cluster might take some time ...\n");
            var assembly = Assembly.GetExecutingAssembly();

            var config = new ClusterConfiguration().LoadFromEmbeddedResource<Program>("Server.xml");

            var system = ActorSystem.Configure()
                .Cluster()
                .From(config)
                .Register(assembly)
                .Done();

            Console.WriteLine("Finished booting cluster...");

            Console.ReadLine();
        }
    }
}