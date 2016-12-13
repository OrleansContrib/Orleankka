![Orleankka Logo](Logo.Wide.jpg)

Orleankka is a functional API for Microsoft Orleans framework. It is highly suitable for scenarios where having composable, uniform communication interface is preferable, such as: CQRS, event-sourcing, re-routing, FSM, etc. 

> References: [intro](https://www.youtube.com/watch?v=07Up88bpl20), [features](https://www.youtube.com/watch?v=FKL-PS8Q9ac), [slides](https://docs.google.com/presentation/d/1brM4SS-uJBRMZs-CdOZoJ0KUgrnPXXwrOXnYgfLL4Nk/edit#slide=id.p4) and [discussion](https://github.com/dotnet/orleans/issues/42).

### Features

+ Message-based api with auto-generation of Orleans' interfaces
+ Special api bindings for C# and F# (DU, Pattern Matching, Tasks, Custom DSL)
+ Simplified programmatic configuration via fluent DSL (client, cluster, azure, embedded)
+ Convenient unit testing kit (stubs, mocks, expectations)
+ Redesigned streams api (actor subscriptions)
+ Switchable actor behaviors with built-in hierarchical FSM
+ Poweful actor invocation pipeline interceptors

### How to install

To install client Orleankka library via NuGet, run this command in NuGet package manager console:

	PM> Install-Package Orleankka

For server-side library:

	PM> Install-Package Orleankka.Runtime

Check out "Getting started" [guide](https://github.com/yevhen/Orleankka/wiki/Getting-Started-%28C%23%29) ([F#](https://github.com/yevhen/Orleankka/wiki/Getting-Started-%28F%23%29)).

### Build sources

Clone repository and run the following in CLI from solution's root folder:

	PM> Nake.bat

This will restore dependencies and build everything in `debug` mode. Run `Nake.bat` with `-T` switch to see available commands.


### Packages

|  |  | [inside]
| ------- |:----:| ---------- |
| Orleankka | [![NuGet](https://img.shields.io/nuget/v/Orleankka.svg?style=flat)](https://www.nuget.org/packages/Orleankka/) | Core and client lib
| Orleankka.Runtime | [![NuGet](https://img.shields.io/nuget/v/Orleankka.Runtime.svg?style=flat)](https://www.nuget.org/packages/Orleankka/) | Server-side runtime lib
| Orleankka.TestKit | [![NuGet](https://img.shields.io/nuget/v/Orleankka.TestKit.svg?style=flat)](https://www.nuget.org/packages/Orleankka.TestKit/) | Unit testing kit
| Orleankka.FSharp | [![NuGet](https://img.shields.io/nuget/v/Orleankka.FSharp.svg?style=flat)](https://www.nuget.org/packages/Orleankka.FSharp/) | F# core and client lib
| Orleankka.FSharp.Runtime | [![NuGet](https://img.shields.io/nuget/v/Orleankka.FSharp.Runtime.svg?style=flat)](https://www.nuget.org/packages/Orleankka.FSharp.Runtime/) | F# server-side runtime lib

### Examples

##### C&#35;

+ "WebScan" [[demo]](Source/Demo.App)
+ TestKit [[demo]](Source/Demo.App.Tests)
+ Event Sourcing 
	+ Idiomatic (CQRS) [[see]](Source/Example.EventSourcing.Idiomatic)
	+ Persistence: GetEventStore [[see]](Source/Example.EventSourcing.Persistence.GES)
	+ Persistence: Streamstone [[see]](Source/Example.EventSourcing.Persistence.Streamstone)
+ Reentrant messages [[rw-x]](Source/Example.Reentrant)
+ Azure cloud service [[hub]](Source/Example.Azure.Cluster)
+ Client-side observers [[chat]](Source/Example.Observers.Chat.Client)
+ Streams [[chat]](Source/Example.Streams.Chat.Server)

##### F&#35;

+ Hello, world! [[demo]](Source/FSharp.Demo.HelloWorld) 
+ Chat  [[demo]](Source/FSharp.Demo.Chat.Server)
+ eCommerce [[demo]](Source/FSharp.Demo.Shop)
+ Worker actors [[see]](Source/FSharp.Demo.Worker)
+ Reentrant messages [[see]](Source/FSharp.Demo.Reentrant)
+ Streams [[chat]](Source/FSharp.Example.Streams.Chat.Actors)

##### Dependency Injection

+ Service Locator [[see]](Source/Demo.App)
+ Autofac [[see]](Source/Example.DependencyInjection.Autofac)

### Documentation

Complete documentation could be found on [wiki](https://github.com/yevhen/Orleankka/wiki).

## Known issues

Integration tests (those using real actor system) won't work with XUnit visual studio runner due to inability to disable shadow copy https://github.com/xunit/visualstudio.xunit/pull/9

## Contributing

Any bug-fix pull request goes without a saying. For new features or modifications, please first create an issue, so we can discuss it before any effort is made.

## Community

[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/OrleansContrib/Orleankka?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## License

Apache 2 License
