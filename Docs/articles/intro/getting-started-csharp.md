This tutorial is intended to give an introduction to using Orleankka by creating a simple greeter actor using C#.

## Set up your project

Start Visual Studio and create a new C# Console Application and reference the following packages:

```PM
PM> Install-Package Orleankka.Runtime
PM> Install-Package Microsoft.Orleans.Server
```

This will install all client and server-side packages required for demo app.

## Create your first actor

First, we need to create a message type that our actor will respond to:

```csharp
namespace ConsoleApplication11
{
    // Create a message types that your actor will respond to
    // and mark them with [Serializable] attribute. This is smilar to
    // object-oriented interface signatures (eg Greet(string who)) but with classes

    [Serializable]
    public class Greet
    {
        public string Who { get; set; }
    }

    [Serializable]
    public class Sleep
    {}
}
```

Once we have the message type, we can create our actor:

```csharp
using System;

using Orleankka;

namespace ConsoleApplication11
{
    // Create custom actor interface and implement IActorGrain
    public interface IGreeter : IActorGrain {}

    // Create actor class by inheriting from ActorGrain and implementing custom actor interface
    public class Greeter : ActorGrain, IGreeter
    {
        // Implement receive function (use pattern matching or any other message matching approach)
        public override Task<object> Receive(object message)
        {
            switch (message)
            {
                case Greet greet:
                    return Result("Hello, {msg.Who}!");

                case Sleep _:
                    Console.WriteLine("Sleeeeping ...");
                    return TaskResult.Done;
                    break;

                default:
                    return Unhandled;
            }
        }
    }
}
```

Now it's time to consume our actor. We need to configure Orleans and then register Orleankka. We'll be using simple localhost configuration suitable for demos.

```csharp
using System;

using Orleankka;
using Orleankka.Playground;

namespace ConsoleApplication11
{
    [Serializable]
    public class Greet
    {
        public string Who { get; set; }
    }

    [Serializable]
    public class Sleep
    {}

    public class Greeter : ActorGrain, IGreeter
    {
        public override Task<object> Receive(object message)
        {
            switch (message)
            {
                case Greet greet:
                    return Result("Hello, {msg.Who}!");

                case Sleep _:
                    Console.WriteLine("Sleeeeping ...");
                    return TaskResult.Done;

                default:
                    return Unhandled;
            }
        }
    }

    class Program
    {
        const string DemoClusterId = "localhost-demo";
        const string DemoServiceId = "localhost-demo-service";
        const int LocalhostSiloPort = 11111;
        const int LocalhostGatewayPort = 30000;
        static readonly IPAddress LocalhostSiloAddress = IPAddress.Loopback;

        static void Main(string[] args)
        {
            var host = await new SiloHostBuilder()
                .Configure(options => {
                    options.ClusterId = DemoClusterId;
                    options.ServiceId = DemoServiceId;
                })
                .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort))
                .ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort)
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .UseOrleankka()  // register Orleankka extension
                .Build();

            // start Orleans server (silo)
            await host.StartAsync();

            var client = new ClientBuilder()
                .ConfigureCluster(options => {
                    options.ClusterId = DemoClusterId;
                    options.ServiceId = DemoServiceId
                })
                .UseStaticClustering(options => options.Gateways.Add(new IPEndPoint(LocalhostSiloAddress, LocalhostGatewayPort).ToGatewayUri()))
                .ConfigureApplicationParts(x => x
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .WithCodeGeneration())
                .UseOrleankka()
                .Build();

            // start (connect) Orleans client
            await client.Connect();

            // get reference to ActorSystem
            var system = client.ActorSystem();

            // get proxy reference for IGreeter actor
            var greeter = system.ActorOf<IGreeter>("id");

            // send query to actor (ie Ask)
            Console.WriteLine(await greeter.Ask<string>(new Greet {Who = "world"}));

            // send command to actor (ie Tell)
            await greeter.Tell(new Sleep());

            Console.Write("\n\nPress any key to terminate ...");
            Console.ReadKey(true);
        }
    }
}
```

That is it. See more examples in a Samples directory.
