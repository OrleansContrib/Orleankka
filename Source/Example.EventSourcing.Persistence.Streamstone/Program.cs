using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Core;
using Orleankka.Meta;
using Orleankka.Playground;

using Microsoft.WindowsAzure.Storage;

namespace Example
{
    public static class Program
    {
        static bool resume;
        static IActorSystem system;

        public static void Main(string[] args)
        {
            resume = args.Length == 1 && args[0] == "resume";

            Console.WriteLine("Make sure you've started Azure storage emulator using \".\\Nake.bat run\"!");
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            var account = CloudStorageAccount.DevelopmentStorageAccount;
            SetupTable(account);

            system = ActorSystem.Configure()
                .Playground()
                .Register(Assembly.GetExecutingAssembly())
                .Serializer<NativeSerializer>()
                .Run<SS.Bootstrap>(new SS.Properties
                {
                    StorageAccount = account.ToString(true),
                    TableName = "ssexample"
                })
                .Done();

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

            await item.Tell(new CreateInventoryItem("XBOX1"));
            await Print(item);

            await item.Tell(new CheckInInventoryItem(10));
            await Print(item);

            await item.Tell(new CheckOutInventoryItem(5));
            await Print(item);
            
            await item.Tell(new RenameInventoryItem("XBOX360"));
            await Print(item);
        }

        static async Task Resume()
        {
            var item = system.ActorOf<InventoryItem>("12345");

            await item.Tell(new CheckInInventoryItem(100));
            await Print(item);

            await item.Tell(new CheckOutInventoryItem(50));
            await Print(item);

            await item.Tell(new CheckOutInventoryItem(45));
            await Print(item);
        }

        static async Task Print(ActorRef item)
        {
            var details = await item.Ask(new GetInventoryItemDetails());

            Console.WriteLine("{0}: {1} {2}",
                                details.Name,
                                details.Total,
                                details.Active ? "" : "(deactivated)");
        }
    }
}
