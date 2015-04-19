using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Core;
using Orleankka.Typed;
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
                .Register(Assembly.GetExecutingAssembly())
                .Serializer<NativeSerializer>()
                .Done();

            Run(system).Wait();

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            system.Dispose();            
            Environment.Exit(0);
        }

        static async Task Run(IActorSystem system)
        {
            var item = system.TypedActorOf<InventoryItem>("12345");

            await item.Call(x => x.Create("XBOX1"));
            await Print(item);

            await item.Call(x => x.CheckIn(10));
            await Print(item);

            await item.Call(x => x.CheckOut(5));
            await Print(item);

            await item.Call(x => x.Rename("XBOX360"));
            await Print(item);

            await item.Call(x => x.Deactivate());
            await Print(item);
        }

        static async Task Print(TypedActorRef<InventoryItem> item)
        {
            var details = await item.Call(x => x.Details());
            
            Console.WriteLine("{0}: {1} {2}", 
                                details.Name, 
                                details.Total, 
                                details.Active ? "" : "(deactivated)");
        }
    }
}
