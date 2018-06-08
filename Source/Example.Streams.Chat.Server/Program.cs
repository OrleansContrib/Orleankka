using System;
using System.Net;
using System.Reflection;

using Orleankka;
using Orleankka.Cluster;

using Orleans.Configuration;
using Orleans.Hosting;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running demo. Booting cluster might take some time ...\n");

            var system = ActorSystem.Configure()
                .Cluster()
                .Builder(b => b
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "test";
                        options.ServiceId = "test";
                    })
                    .UseLocalhostClustering()
                    .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                    .AddSimpleMessageStreamProvider("sms")
                    .AddMemoryGrainStorage("PubSubStore"))
                .Assemblies(
                    typeof(Join).Assembly, 
                    Assembly.GetExecutingAssembly())
                .Done();

            system.Start().Wait();

            Console.WriteLine("Finished booting cluster...");
            Console.ReadLine();
        }
    }
}