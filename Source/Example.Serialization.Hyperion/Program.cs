using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;
using Orleankka.Client;
using Orleankka.Cluster;

using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;

namespace Example
{
    public static class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Running example. Booting cluster might take some time ...\n");

            var config = ClusterConfiguration
                .LocalhostPrimarySilo()
                .UseSerializer<HyperionSerializer>();

            config.AddMemoryStorageProvider("PubSubStore");
            config.AddSimpleMessageStreamProvider("sms");

            var host = await new SiloHostBuilder()
                .UseConfiguration(config)
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .ConfigureOrleankka()
                .Start();

            var client = await host.Connect(x => x.UseSerializer<HyperionSerializer>());
            await Run(client.ActorSystem());

            Console.WriteLine("\nPress any key to terminate ...");
            Console.ReadKey(true);

            host.Dispose();
            Environment.Exit(0);
        }

        static async Task Run(IClientActorSystem system)
        {
            var some = system.ActorOf<SomeActor>("some");
            var another = system.ActorOf<AnotherActor>("another");

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