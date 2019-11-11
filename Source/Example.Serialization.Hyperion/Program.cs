using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;
using Orleankka.Client;
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
                .Client(x => x.Configuration.SerializationProviders.Add(typeof(HyperionSerializer).GetTypeInfo()))
                .Cluster(x =>
                {
                    x.UseInMemoryPubSubStore();
                    x.Configuration.Globals.SerializationProviders.Add(typeof(HyperionSerializer).GetTypeInfo());
                })
                .Assemblies(Assembly.GetExecutingAssembly())
                .Done();

            system.Start().Wait();
            Run(system).Wait();

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            system.Dispose();
            Environment.Exit(0);
        }

        static async Task Run(IClientActorSystem system)
        {
            var some = system.ActorOf<ISomeActor>("some");
            var another = system.ActorOf<IAnotherActor>("another");

            var actor = await some.Ask(new GetSelf());
            var stream = await some.Ask(new GetStream());

            var item1 = new Item {Name = "PS3", Paid = DateTimeOffset.UtcNow.AddHours(-2), Price = 600};
            var item2 = new Item {Name = "XBOX", Paid = DateTimeOffset.UtcNow.AddHours(-9), Price = 500};

            await another.Tell(new Notify{Observer = actor, Item = item1});
            await another.Tell(new Push{Stream = stream, Item = item2});

            var received = await some.Ask(new GetReceived());
            Verify(received[0], item1);
            Verify(received[1], item2);

            using (var observable = await system.CreateObservable())
            {
                Item observed = null;
                using (observable.Subscribe<Item>(x => observed = x))
                {
                    var item3 = new Item {Name = "Nintendo", Paid = DateTimeOffset.UtcNow, Price = 300 };
                    await another.Tell(new Notify {Observer = observable.Ref, Item = item3});

                    Thread.Sleep(2000);
                    Verify(observed, item3);
                }
            }
        }

        static void Verify(Item actual, Item expected)
        {
            Console.WriteLine($"Expected: {expected.Name} - {expected.Price} : {expected.Paid:f}");
            Console.WriteLine($"Actual:   {actual.Name} - {actual.Price} : {actual.Paid:f}");
            Console.WriteLine();
        }
    }
}