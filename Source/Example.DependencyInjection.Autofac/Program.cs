using System;
using System.Reflection;
using System.Threading.Tasks;

using Autofac;

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

            var setup = new Action<ContainerBuilder>(builder =>
            {
                builder.RegisterType<SomeService>()
                       .AsImplementedInterfaces()
                       .WithParameter("connectionString", Settings.Default.ConnectionString)
                       .SingleInstance();

                builder.RegisterType<DIActor>();
            });

            var system = ActorSystem
                .Configure()
                .Playground()
                .Cluster(x => x.Activator(new AutofacActorActivator(setup)))
                .Assemblies(Assembly.GetExecutingAssembly())
                .Done();

            system.Start().Wait();
            Run(system).Wait();

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

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
