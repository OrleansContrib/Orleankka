using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using FSM.Domain;
using FSM.Domain.Commands;
using FSM.Domain.Queries;
using FSM.Infrastructure;

using Orleankka;
using Orleankka.Playground;

namespace FSM
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");
            Console.WriteLine("Make sure you've started local GES node using \".\\Nake.bat run\"!");

            var system = ActorSystem.Configure()
                                    .Playground()
                                    .Bootstrapper<ES.Bootstrap>()
                                    .Assemblies(Assembly.GetExecutingAssembly())
                                    .Done();

            system.Start();
            Run(system).Wait();

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            system.Dispose();
            Environment.Exit(0);
        }

        private static async Task Run(IActorSystem system)
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

        private static async Task Print(ActorRef item)
        {
            var details = await item.Ask<InventoryItemDetails>(new GetDetails());

            Console.WriteLine("{0}: {1} {2}",
                              details.Name,
                              details.Total,
                              details.Active ? "" : "(deactivated)");
        }
    }
}