using System;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Client;
using Orleankka.Cluster;
using Orleankka.Meta;

using Orleans;
using Orleans.Hosting;

namespace Example
{
    public static class Program
    {   
        public static async Task Main()
        {
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            var host = await new SiloHostBuilder()
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .ConfigureOrleankka()
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
            var item = system.ActorOf<IInventoryItem>("12345");

            await item.Tell(new Create("XBOX1"));
            await Print(item);

            await item.Tell(new CheckIn(10));
            await Print(item);

            await item.Tell(new CheckOut(5));
            await Print(item);

            await item.Tell(new Rename("XBOX360"));
            await Print(item);

            await item.Tell(new DeactivateItem());
            await Print(item);

            var inventory = system.ActorOf<IInventory>("#");

            var items = await inventory.Ask(new GetInventoryItems());
            Console.WriteLine($"\n# of items in inventory: {items.Length}");
            Array.ForEach(items, Print);

            var total = await inventory.Ask(new GetInventoryItemsTotal());
            Console.WriteLine($"\nTotal of all items inventory: {total}");
        }

        static async Task Print(ActorRef item) => Print(await item.Ask(new GetDetails()));

        static void Print(InventoryItemDetails details)
        {
            Console.WriteLine("{0}: {1} {2}",
                details.Name,
                details.Total,
                details.Active ? "" : "(deactivated)");
        }
    }
}
