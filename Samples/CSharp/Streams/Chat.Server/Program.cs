using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Orleans.Hosting;
using Orleankka.Cluster;

namespace Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Running demo. Booting cluster might take some time ...\n");

            var host = new HostBuilder()
                .UseOrleans(c => c.UseLocalhostClustering()
                    .AddMemoryGrainStorage("PubSubStore")
                    .AddMemoryGrainStorageAsDefault()
                    .AddMemoryStreams("sms"))
                .UseOrleankka()
                .Build();

            await host.StartAsync();

            Console.WriteLine("Finished booting cluster...");
            Console.ReadLine();
        }
    }
}