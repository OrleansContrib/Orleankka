### Orleankka

Orleankka is a complementary API for Microsoft Orleans framework. Orleankka's API is based on [message passing](http://en.wikipedia.org/wiki/Message_passing) style of communication. Orleankka was developed specifically for scenarios, where having a uniform communication interface would payoff in terms of reduced code repetition (DRY, SRP) and respectively, increased code clarity. The API was thoroughly crafted to be as convenient to use from both, imperative object-oriented languages, like C#, as well as from functional, such as F#.

### Why?

Out-of-the box, Microsoft Orleans comes with static code generator and support for non-uniform custom interfaces, very similar to WCF. This approach fits nicely into an object-oriented paradigm and is good enough for majority of applications and use-cases.

Unfortunately, it doesn't play well when combined with [Event Sourcing](https://msdn.microsoft.com/en-us/library/dn589792.aspx) approach to state persistence. An interplay between non-uniform and uniform interface leads to code duplication and is generally error-prone. That can be remedied by introduction of a catch-all function, where all requests received by an actor could be intercepted (AOP). Sadly, but such highly useful higher-order function is not exposed by the framework.   

Orleankka is an attempt to fix that problem in a generic way. It turned out that introduction of uniform communication interface, also fixed a lot of other small-to-medium annoyances, constraints and even some of the major limitations, present in current Orleans programming model.

> References: [video](https://www.youtube.com/watch?v=07Up88bpl20), [slides](https://docs.google.com/presentation/d/1brM4SS-uJBRMZs-CdOZoJ0KUgrnPXXwrOXnYgfLL4Nk/edit#slide=id.p4) and [discussion](https://github.com/dotnet/orleans/issues/42).

### Goals

- __No sacrifices__. Full feature parity with native Orleans api and more.
- __Side-by-side execution__. Can mix uniform actors with native Orleans' grains.
- __Simplicity__. For both simple and complex scenarios.
- __Designed for testability__. Inversion of control - you drive the framework.  
- __Annoyance-free__. Eliminate noise induced by Orleans' native programming model.
- __Low friction__. Within syntax limits of major programming languages (C#/F#).

### Features

##### Runtime

+ Dynamic proxy creation
+ Pluggable serialization protocols
+ Dependency injection support
+ Simplified configuration via fluent DSL (client, cluster, azure, embedded)
+ Programmable from any .NET language

##### Actors

+ Higher-order catch-all function (AOP)
+ Message handler auto-wiring
+ Reentrant messages
+ Automatic GC with configurable keep-alive timeouts
+ Runtime independence (isolated testing)
+ **New!** Typed actors 

##### Other

+ Convenient unit testing kit (stubs, mocks, expectations)
+ Special api for F# (DU, Pattern Matching, Tasks, Custom DSL)
+ Support of non-uniform and lambda-based message handlers (noise reduction for C#)
+ Reactive Extensions (RX) support (client-side observers)
+ Frictionless exception handling (automatic unwrapping of AggregateException)

### Roadmap

+ Extensible actor prototypes
+ Additional serialization formats (ProtoBuf, Bond?)
+ FSM support
+ Http endpoint
+ More examples
+ Reference documentation

### How to install [![NuGet](https://img.shields.io/nuget/v/Orleankka.svg?style=flat)](https://www.nuget.org/packages/Orleankka/)

To install Orleankka via NuGet, run this command in NuGet package manager console:

	PM> Install-Package Orleankka

Check out "Getting started" [guide](https://github.com/yevhen/Orleankka/wiki/Getting-Started-%28C%23%29) ([F#](https://github.com/yevhen/Orleankka/wiki/Getting-Started-%28F%23%29)).

### Add-ons

|  |  | [inside]
| ------- |:----:| ---------- |
| Orleankka.FSharp | [![NuGet](https://img.shields.io/nuget/v/Orleankka.FSharp.svg?style=flat)](https://www.nuget.org/packages/Orleankka.FSharp/) | Special api for F#
| Orleankka.TestKit | [![NuGet](https://img.shields.io/nuget/v/Orleankka.TestKit.svg?style=flat)](https://www.nuget.org/packages/Orleankka.TestKit/) | Unit testing kit
| Orleankka.Azure | [![NuGet](https://img.shields.io/nuget/v/Orleankka.Azure.svg?style=flat)](https://www.nuget.org/packages/Orleankka.Azure/) | Fluent configuration for Azure

### Examples

+ Event Sourcing [[basic]](Source/Example.EventSourcing)
+ Reentrant messages [[rw-x]](Source/Example.Reentrant)
+ Azure cloud service [[hub]](Source/Example.Azure.Cluster)
+ Testing actors in isolation [[kit]](Source/Demo.App.Tests/TopicFixture.cs)
+ (F#) "Hello, world!", "Chat", "eShop" [[see]](Source/)

##### Serialization

+ .NET binary [default] 
+ Orleans native (codegened) [[built-in]](Source/Example.Serialization.Native.App/Program.cs#L19)
+ Newtonsoft.JSON [[see]](Source/Orleankka.Tests/Utility/JsonSerializer.cs)

##### Dependency Injection

+ Service Locator [[see]](Source/Demo.App)
+ Unity   [[see]](Source/Example.DI.Unity) 
+ NInject [[see]](Source/Example.DI.NInject)

### Documentation

Complete documentation could be found on [wiki](https://github.com/yevhen/Orleankka/wiki).

## Contributing

Any bug-fix pull request goes without a saying. For new features or modifications, please first create an issue, so we can discuss it before any effort is made. Add-ons, like new serialization protocols, DI container support, etc - are highly welcome!

## Community

[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/yevhen/Orleankka?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## License

Apache 2 License
