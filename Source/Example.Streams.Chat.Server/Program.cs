using System;
using System.Reflection;
using System.Threading.Tasks;

using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Orleankka.Cluster;

namespace Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Running demo. Booting cluster might take some time ...\n");

            var config = ClusterConfiguration.LocalhostPrimarySilo();
            config.AddMemoryStorageProvider("PubSubStore");
            config.AddSimpleMessageStreamProvider("sms");

            var host = new SiloHostBuilder()
                .UseConfiguration(config)
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .AddApplicationPart(typeof(Join).Assembly)
                    .WithCodeGeneration())
                .ConfigureOrleankka()
                .Build();

            await host.StartAsync();

            Console.WriteLine("Finished booting cluster...");
            Console.ReadLine();
        }
    }
}