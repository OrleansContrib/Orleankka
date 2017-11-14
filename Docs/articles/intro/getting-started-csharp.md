This tutorial is intended to give an introduction to using Orleankka by creating a simple greeter actor using C#.

## Set up your project

Start Visual Studio and create a new C# Console Application.
Once we have our console application, we need to open up the Package Manager Console and type:

```PM
PM> Install-Package Orleankka
```
## Create your first actor

First, we need to create a message type that our actor will respond to:

```csharp
using System;

using Orleankka;

namespace ConsoleApplication11
{
    // Create an (immutable) message type that your actor will respond to
    // and mark it with [Serializable] attribute
    [Serializable]
    public class Greet
    {
        public Greet(string who)
        {
            Who = who;
        }
        public string Who { get; }
    }
}
```

Once we have the message type, we can create our actor:

```csharp
using System;

using Orleankka;

namespace ConsoleApplication11
{
    [Serializable]
    public class Greet
    {
        public Greet(string who)
        {
            Who = who;
        }
        public string Who { get; }
    }

    // Create actor class by inheriting from Actor
    public class GreetingActor : Actor
    {
        // Declare hander method for message type defined above
        void On(Greet greet) { Console.WriteLine("Hello {0}", greet.Who); }
    }
}
```

Now it's time to consume our actor. We do so by configuring and starting `ActorSystem` and using `ActorOf` method to get proxy reference to our actor. For this tutorial we'll use playground actor system configuration suitable for demos, but in real project you will use more elaborate setups, you can learn about other possible deployment and configuration options in "Configuration" section.

```csharp
using System;

using Orleankka;
using Orleankka.Playground;

namespace ConsoleApplication11
{
    [Serializable]
    public class Greet
    {
        public Greet(string who)
        {
            Who = who;
        }
        public string Who { get; }
    }

    public class GreetingActor : Actor
    {
        void On(Greet greet) { Console.WriteLine("Hello {0}", greet.Who); }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Configure a new actor system (a container for your actors)
            var system = ActorSystem.Configure()
                // Use playground actor system configuration suitable for demos
                .Playground()
                // Use special extension method to register all actors in a given assembly
                .Register(Assembly.GetExecutingAssembly())
                // Complete configuration and start actor system
                .Done();

            // Note: It can take several seconds to start an actor system
            
            // Get a proxy reference to an actor.
            // You don't need to pre-create an actor -
            // it will be automatically activated by the system
            var greeter = system.ActorOf<GreetingActor>("greeter");

            // Send a message to the actor and 
            // wait until message is processed
            greeter.Tell(new Greet("World")).Wait();
            
            Console.ReadLine();
        }
    }
}
```

That is it, your actor is now ready to consume messages.