using System;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;
using Orleankka.Client;
using Orleankka.Cluster;

using Orleans;
using Orleans.Hosting;
using Microsoft.Azure.Cosmos.Table;

namespace Example
{
    public static class Program
    {
        static bool resume;
        static IClientActorSystem system;

        public static async Task Main(string[] args)
        {
            resume = args.Length == 1 && args[0] == "resume";

            Console.WriteLine("Make sure you've started Azure storage emulator!");
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            var account = CloudStorageAccount.DevelopmentStorageAccount;
            SS.Table = await SetupTable(account);

            var host = await new SiloHostBuilder()
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .UseOrleankka()
                .Start();

            var client = await host.Connect();
            system = client.ActorSystem();

            try
            {
                (resume ? Resume() : Run()).Wait();
            }
            catch (AggregateException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.InnerException.Message);
            }

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            host.Dispose();
            Environment.Exit(0);
        }

        static async Task<CloudTable> SetupTable(CloudStorageAccount account)
        {
            var table = account
                .CreateCloudTableClient()
                .GetTableReference("ssexample");

            if (!resume)
                await table.DeleteIfExistsAsync();

            await table.CreateIfNotExistsAsync();
            return table;
        }

        static async Task Run()
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
        }

        static async Task Resume()
        {
            var item = system.ActorOf<IInventoryItem>("12345");

            await item.Tell(new CheckIn(100));
            await Print(item);

            await item.Tell(new CheckOut(50));
            await Print(item);

            await item.Tell(new CheckOut(45));
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
