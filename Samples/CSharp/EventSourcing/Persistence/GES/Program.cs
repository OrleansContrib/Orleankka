using System;
using System.Net;
using System.Threading.Tasks;

using EventStore.ClientAPI;

using Orleankka;
using Orleankka.Cluster;
using Orleankka.Meta;

namespace Example
{
    using Microsoft.Extensions.Hosting;

    public static class Program
    {   
        public static async Task Main()
        {
            Console.WriteLine("Make sure you've started local GES node using \".\\Nake.bat run\"!");
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            ES.Connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            await ES.Connection.ConnectAsync();

            var host = await new HostBuilder()
                .UseOrleankka()
                .StartServer();

            await Run(host.ActorSystem());

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
