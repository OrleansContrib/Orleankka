### Orleankka

Orleankka is a functional API for Microsoft Orleans framework. It is highly suitable for scenarios where having composable, uniform communication interface is preferable, such as: CQRS, event-sourcing, routing, FSM, etc. 

The API was thoroughly crafted to be as convenient to use from both, imperative object-oriented languages, like C#, as well as from functional, such as F#. 

Orleankka is not just a translation layer on top of Orleans. Besides improved language support, Orleannka fixes a lot of other small-to-medium annoyances, constraints and some of the major limitations, present in current Orleans programming model.

> References: [video](https://www.youtube.com/watch?v=07Up88bpl20), [slides](https://docs.google.com/presentation/d/1brM4SS-uJBRMZs-CdOZoJ0KUgrnPXXwrOXnYgfLL4Nk/edit#slide=id.p4) and [discussion](https://github.com/dotnet/orleans/issues/42).

### Features

##### Runtime

+ Pluggable serialization protocols
+ Dependency injection support
+ Simplified configuration via fluent DSL (client, cluster, azure, embedded)
+ Programmable from any .NET language
+ **New!** Streams
 
##### Actors

+ Message interception via higher-order catch-all function (AOP)
+ Automatic GC with configurable keep-alive timeouts
+ Runtime independence (isolated testing)
+ Reentrant messages
+ Typed actors (C#)
+ Lambda-based message handlers (C#)
+ Special api for F# (DU, Pattern Matching, Tasks, Custom DSL)

##### Other

+ Convenient unit testing kit (stubs, mocks, expectations)
+ Push-based notifications with observers
+ Reactive Extensions (RX) support (client-side observers only)
+ Improved exception handling

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

##### C#

+ Event Sourcing [[basic]](Source/Example.EventSourcing)
+ Reentrant messages [[rw-x]](Source/Example.Reentrant)
+ Azure cloud service [[hub]](Source/Example.Azure.Cluster)
+ Testing actors in isolation [[kit]](Source/Demo.App.Tests/TopicFixture.cs)

##### F# Demo

+ Hello, world! [[see]](Source/) 
+ Chat  [[see]](Source/)
+ eCommerce [[see]](Source/)

##### Serialization

+ .NET binary [default]
+ Orleans native (codegened) [[built-in]](Source/Example.Serialization.Native)
+ Newtonsoft.JSON [[see]](Source/Example.Serialization.JSON)

##### Dependency Injection

+ Service Locator [[see]](Source/Demo.App)
+ Autofac [[see]](Source/Example.DependencyInjection.Autofac)

### Documentation

Complete documentation could be found on [wiki](https://github.com/yevhen/Orleankka/wiki).

## Contributing

Any bug-fix pull request goes without a saying. For new features or modifications, please first create an issue, so we can discuss it before any effort is made. Add-ons, like new serialization/communication protocols, DI container support, FSM, etc - are highly welcome!

## Community

[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/yevhen/Orleankka?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## License

Apache 2 License
