using System;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;
using Orleankka.Playground;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");
            Console.WriteLine("Make sure you've started local GES node using \".\\Nake.bat run\"!");
            Console.WriteLine("You may need to first run \".\\Nake.bat restore\" to download GES binaries\n");

            var system = ActorSystem.Configure()
                .Playground()
                .Bootstrapper<ES.Bootstrap>()
                .Assemblies(Assembly.GetExecutingAssembly())
                .Done();

            system.Start();
            Run(system).Wait();

            Console.WriteLine("\nTry running this sample again or change item id to start fresh");
            Console.WriteLine("Press any key to terminate ...");
            Console.ReadKey(true);

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