using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using EventStore.ClientAPI;

using Orleankka;
using Orleankka.Client;
using Orleankka.Cluster;
using Orleankka.Meta;

using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;

namespace Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");
            Console.WriteLine("Make sure you've started local GES node using \".\\Nake.bat run\"!");
            Console.WriteLine("You may need to first run \".\\Nake.bat restore\" to download GES binaries\n");

            ES.Connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            await ES.Connection.ConnectAsync();

            var host = await new SiloHostBuilder()
                .UseConfiguration(ClusterConfiguration.LocalhostPrimarySilo())
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .ConfigureOrleankka()
                .Start();

            var client = await host.Connect();
            await Run(client.ActorSystem());

            Console.WriteLine("\nTry running this sample again or change item id to start fresh");
            Console.WriteLine("Press any key to terminate ...");
            Console.ReadKey(true);

            host.Dispose();
            Environment.Exit(0);
        }

        static async Task Run(IActorSystem system)
        {
            var item = system.ActorOf<InventoryItem>("12345");
            await Print(item);

            await Tell(item, new Create("XBOX1"));
            await Print(item);

            await Tell(item, new CheckIn(10));
            await Print(item);

            await Tell(item, new CheckOut(5));
            await Print(item);

            await Tell(item, new Rename("XBOX360"));
            await Print(item);

            await Tell(item, new Deactivate());
            await Print(item);
        }

        static async Task Tell(ActorRef item, Command cmd)
        {
            try
            {
                await item.Tell(cmd);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task Print(ActorRef item)
        {
            try
            {
                var details = await item.Ask<InventoryItemDetails>(new GetDetails());

                Console.WriteLine("{0}: '{1}' ({2} pcs) {3}",
                    item.Path.Id,
                    details.Name ?? "-",
                    details.Total,
                    details.Active ? "" : "[inactive]");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}