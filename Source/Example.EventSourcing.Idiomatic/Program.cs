using System;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;
using Orleankka.Playground;

namespace Example
{
    public static class Program
    {   
        public static void Main()
        {
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            var system = ActorSystem.Configure()
                .Playground()
                .UseInMemoryPubSubStore()
                .Register(Assembly.GetExecutingAssembly())
                .Done();

            system.Start();
            Run(system).Wait();

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            system.Dispose();            
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

            var inventory = system.ActorOf<Inventory>("#");

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
