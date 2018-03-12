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
                .UseSerializer<ProtobufSerializer>()
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .ConfigureOrleankka()
                .Start();

            var client = await host.Connect(x => x.UseSerializer<ProtobufSerializer>());
            await Run(client.ActorSystem());

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            host.Dispose();
            Environment.Exit(0);
        }

        static async Task Run(IActorSystem system)
        {
            var item = system.ActorOf<IInventoryItem>("12345");

            await item.Tell(new Create {Name = "XBOX1"});
            await Print(item);

            await item.Tell(new CheckIn {Quantity = 10});
            await Print(item);

            await item.Tell(new CheckOut {Quantity = 5});
            await Print(item);

            await item.Tell(new Rename {NewName = "XBOX360"});
            await Print(item);

            await item.Tell(new DeactivateItem());
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