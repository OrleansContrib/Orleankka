![Orleankka Logo](Logo.Wide.jpg)

Orleankka is a functional extension for Microsoft Orleans framework. It provides a message-based API similar to Akka/Erlang, carefully layered on top of the Orleans (that's what in a name). Orleankka is an excellent choice for use-cases which can benefit from composable, uniform communication interface, such as CQRS, event-sourcing, FSM, etc.

> References: [intro](https://www.youtube.com/watch?v=07Up88bpl20), [features](https://www.youtube.com/watch?v=FKL-PS8Q9ac), [slides](https://docs.google.com/presentation/d/1brM4SS-uJBRMZs-CdOZoJ0KUgrnPXXwrOXnYgfLL4Nk/edit#slide=id.p4) and [discussion](https://github.com/dotnet/orleans/issues/42).

### Features

+ Message-based API with zero performance overhead
+ Custom F# DSL and bindings (DU, Pattern Matching, Tasks)
+ Convenient unit testing kit (stubs, mocks, expectations)
+ Simplified streams API (actor subscriptions)
+ Declarative regex-based stream subscriptions (great for CQRS/ES projections)
+ Content-based filtering support for stream subscriptions (imperative/declarative)
+ Switchable actor behaviors with built-in hierarchical FSM (behaviors)
+ Poweful actor/proxy middlewares (interceptors)

### How to install

To install client Orleankka library via NuGet, run this command in NuGet package manager console:

	PM> Install-Package Orleankka

For server-side library:

	PM> Install-Package Orleankka.Runtime

Check out "Getting started" guide: [C#](https://github.com/OrleansContrib/Orleankka/wiki/Getting-Started-CSharp)
, [F#](https://github.com/OrleansContrib/Orleankka/wiki/Getting-Started-FSharp).

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
+ Client-side observers [[chat]](Source/Example.Observers.Chat.Client)
+ Streams [[chat]](Source/Example.Streams.Chat.Server)

##### F&#35;

+ Hello, world! [[demo]](Source/FSharp.Demo.HelloWorld) 
+ Chat  [[client]](Source/FSharp.Example.Observers.Chat.Client) [[server]](Source/FSharp.Example.Observers.Chat.Server) [[shared]](Source/FSharp.Example.Observers.Chat.Shared)
+ eCommerce [[demo]](Source/FSharp.Demo.Shop)
+ Reentrant messages [[demo]](Source/FSharp.Example.Reentrant)
+ Streams [[client]](Source/FSharp.Example.Streams.Chat.Client) [[server]](Source/FSharp.Example.Streams.Chat.Server) [[shared]](Source/FSharp.Example.Streams.Chat.Shared)

### Documentation

Documentation can be found [here](http://orleanscontrib.github.io/Orleankka/).

## Community

+ Join [Gitter](https://gitter.im/OrleansContrib/Orleankka) chat
+ Follow the [@Orleankka](https://twitter.com/Orleankka) Twitter account for announcements

## License

Apache 2 License
