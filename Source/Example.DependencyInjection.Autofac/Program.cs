using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Playground;

namespace Example
{
    using Properties; 

    class Program
    {
        public static void Main()
        {
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            var configuration = new Dictionary<string, string>
            {
                {"ConnectionString", Settings.Default.ConnectionString}
            };

            var system = ActorSystem.Configure()
                .Playground()
                .Activator<Activator>(configuration)
                .Register(Assembly.GetExecutingAssembly())
                .Done();

            Run(system).Wait();

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            system.Dispose();            
            Environment.Exit(0);
        }

        static async Task Run(IActorSystem system)
        {
            var a = system.ActorOf<DIActor>("A-123");
            var b = system.ActorOf<DIActor>("B-456");

            await a.Tell("Hello");
            await b.Tell("Bueno");
        }
    }
}
