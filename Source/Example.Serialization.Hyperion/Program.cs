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
                .TweakClient(c => c.SerializationProviders.Add(typeof(HyperionSerializer).GetTypeInfo()))
                .TweakCluster(c => c.Globals.SerializationProviders.Add(typeof(HyperionSerializer).GetTypeInfo()))
                .Assemblies(Assembly.GetExecutingAssembly())
                .Done();

            system.Start();
            Run(system).Wait();

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            system.Stop();
            Environment.Exit(0);
        }

        static async Task Run(IActorSystem system)
        {
            var item = system.ActorOf<InventoryItem>("12345");

            await item.Tell(new Create {Name = "XBOX1"});
            await Print(item);

            await item.Tell(new CheckIn {Quantity = 10});
            await Print(item);

            await item.Tell(new CheckOut {Quantity = 5});
            await Print(item);

            await item.Tell(new Rename {NewName = "XBOX360"});
            await Print(item);

            await item.Tell(new Deactivate());
            await Print(item);
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