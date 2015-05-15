using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Core;
using Orleankka.Meta;
using Orleankka.Playground;
using Autofac;

namespace Example.DependencyInjection.Autofac
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            var configureAction = new Action<ContainerBuilder>(builder =>
            {
                builder.RegisterType<SomeService>().AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<InventoryItem>();
            });

            var system = ActorSystem.Configure()
                .Playground()
                .Activator<AutofacActorActivator>(new Dictionary<string, object>
                {
                    {AutofacActorActivator.ContainerBuilderActionPropertyKey, configureAction}
                })
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
            var item = system.ActorOf<InventoryItem>("12345");

            await item.Tell(new CreateInventoryItem("XBOX1"));
            await Print(item);

            await item.Tell(new CheckInInventoryItem(10));
            await Print(item);

            await item.Tell(new CheckOutInventoryItem(5));
            await Print(item);

            await item.Tell(new RenameInventoryItem("XBOX360"));
            await Print(item);

            await item.Tell(new DeactivateInventoryItem());
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
