using System;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Client;
using Orleankka.Cluster;
using Orleankka.Meta;

using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;

namespace Example
{
    public static class Program
    {   
        public static async Task Main()
        {
            Console.WriteLine("Make sure you've started local GES node using \".\\Nake.bat run\"!");
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            var host = await new SiloHostBuilder()
                .UseConfiguration(ClusterConfiguration.LocalhostPrimarySilo())
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .ConfigureOrleankka(x => x.Bootstrapper<ES.Bootstrap>())
                .Start();

            var client = await host.Connect();
            await Run(client.ActorSystem());

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            host.Dispose();
            Environment.Exit(0);
        }

        static async Task Run(IActorSystem system)
        {
            var item = system.ActorOf<InventoryItem>("12345");

            await item.Tell(new Create("XBOX1"));
            await Print(item);

            await item.Tell(new CheckIn(10));
            await Print(item);

            await item.Tell(new CheckOut(5));
            await Print(item);

            await item.Tell(new Rename("XBOX360"));
            await Print(item);

            await item.Tell(new Deactivate());
            await Print(item);
        }

        static async Task Print(ActorRef item)
        {
            var details = await item.Ask(new GetDetails());

            Console.WriteLine("{0}: {1} {2}",
                                details.Name,
                                details.Total,
                                details.Active ? "" : "(deactivated)");
        }
    }
}
