using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Embedded;
using Orleankka.Meta;
using Orleankka.Playground;

using Microsoft.WindowsAzure.Storage;

namespace Example
{
    public static class Program
    {
        static bool resume;
        static EmbeddedActorSystem system;

        public static void Main(string[] args)
        {
            resume = args.Length == 1 && args[0] == "resume";

            Console.WriteLine("Make sure you've started Azure storage emulator using \".\\Nake.bat run\"!");
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            var account = CloudStorageAccount.DevelopmentStorageAccount;
            SetupTable(account);

            system = ActorSystem.Configure()
                .Playground()
                .Run<SS.Bootstrap>(new SS.Properties
                {
                    StorageAccount = account.ToString(true),
                    TableName = "ssexample"
                })
                .Register(Assembly.GetExecutingAssembly())
                .Done();

            system.Start();

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

            system.Dispose();
            Environment.Exit(0);
        }

        static void SetupTable(CloudStorageAccount account)
        {
            var table = account
                .CreateCloudTableClient()
                .GetTableReference("ssexample");

            if (!resume)
                table.DeleteIfExists();

            table.CreateIfNotExists();
        }

        static async Task Run()
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
        }

        static async Task Resume()
        {
            var item = system.ActorOf<InventoryItem>("12345");

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
