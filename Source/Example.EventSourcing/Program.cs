using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;
using Orleankka.Playground;
using Orleankka.Utility;

namespace Example
{
    public static class Program
    {   
        public static void Main()
        {
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            var system = ActorSystem.Configure()
                .Playground()
                .Register(Assembly.GetExecutingAssembly())
                .Serializer<JsonSerializer>()
                .Done();

            Run(system).Wait();

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            system.Dispose();            
            Environment.Exit(0);
        }

        static async Task Run(IActorSystem system)
        {
            var item = system.ActorOf<InventoryItem>("12345");

            await item.Tell(new CreateInventoryItem {Name = "XBOX1"});
            await Print(item);

            await item.Tell(new CheckInInventoryItem {Quantity = 10});
            await Print(item);

            await item.Tell(new CheckOutInventoryItem {Quantity = 5});
            await Print(item);

            await item.Tell(new DeactivateInventoryItem());
            await Print(item);
        }

        static async Task Print(ActorRef item)
        {
            var details = await item.Ask(new GetInventoryItemDetails());
            Console.WriteLine("{0}: {1} {2}", details.Name, details.Total, details.Active ? "" : "(deactivated)");
        }
    }
}
