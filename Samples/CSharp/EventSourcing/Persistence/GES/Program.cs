using System;
using System.Threading.Tasks;

using EventStore.Client;
using Microsoft.Extensions.Hosting;

using Orleankka;
using Orleankka.Cluster;
using Orleankka.Meta;

namespace Example
{
    public static class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Make sure you've started local GES node using \".\\Nake.bat run\"!");
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            ConfigureEventStoreClient();

            var host = await new HostBuilder()
                .UseOrleankka()
                .StartServer();

            await Run(host.ActorSystem());

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            host.Dispose();
            Environment.Exit(0);
        }

        static void ConfigureEventStoreClient()
        {
            const string connectionString = "esdb+discover://127.0.0.1:2113?tls=false&keepAliveTimeout=10000&keepAliveInterval=10000";
            ES.Client = new EventStoreClient(EventStoreClientSettings.Create(connectionString));
        }

        static async Task Run(IActorSystem system)
        {
            var item = system.ActorOf<IInventoryItem>(Guid.NewGuid().ToString("N"));

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
