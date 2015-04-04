### Orleankka

[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/yevhen/Orleankka?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Build status](https://ci.appveyor.com/api/projects/status/qtfih702sfxcsyt6?svg=true)](https://ci.appveyor.com/project/yevhen/orleankka)
[![NuGet](https://img.shields.io/nuget/v/Orleankka.svg?style=flat)](https://www.nuget.org/packages/Orleankka/)

Orleankka is a complementary API for Microsoft Orleans framework. Orleankka's API is based on [message passing](http://en.wikipedia.org/wiki/Message_passing) style of communication. Orleankka was developed specifically for scenarios, where having a uniform communication interface would payoff in terms of reduced code repetition (DRY, SRP) and respectively, increased code clarity. The API was thoroughly crafted to be as convenient to use from both, imperative object-oriented languages, like C#, as well as from functional, such as F#.


### Why?

Out-of-the box, Microsoft Orleans comes with static code generator and support for non-uniform custom interfaces, very similar to WCF. This approach fits nicely into an object-oriented paradigm and is good enough for majority of applications and use-cases.

Unfortunately, it doesn't play well when combined with [Event Sourcing](https://msdn.microsoft.com/en-us/library/dn589792.aspx) approach to state persistence. An interplay between non-uniform and uniform interface leads to code duplication and is generally error-prone. That can be remedied by introduction of a catch-all function, where all requests received by an actor could be intercepted (AOP). Sadly, but such highly useful higher-order function is not exposed by the framework.   

Orleankka is an attempt to fix that problem in a generic way. It turned out that introduction of uniform communication interface, also fixed a lot of other small-to-medium annoyances, constraints and even some of the major limitations, present in current Orleans programming model.

> References: [video](https://www.youtube.com/watch?v=07Up88bpl20), [slides](https://docs.google.com/presentation/d/1brM4SS-uJBRMZs-CdOZoJ0KUgrnPXXwrOXnYgfLL4Nk/edit#slide=id.p4) and [discussion](https://github.com/dotnet/orleans/issues/42).

### Goals

- __No sacrifices__. Absolute feature parity with native Orleans api and more.
- __Side-by-side execution__. Can mix Orleankka's uniform actors with native Orleans' grains.
- __Simplicity__. For both simple and complex scenarios.
- __Designed for testability__. No need to introduce any synthetic classes to gain testability. 
- __Annoyance-free__. Eliminate all noise and repetitiveness of Orleans' native programming model.
- __Low friction__. Within a syntax limits of major programming languages (F#/C#).

### Features

+ No static code generation
+ Pluggable serialization protocols
+ Usable from any CLR language (not only C#/VB)
+ Proper support for F# (DU, Pattern Matching, Tasks)
+ Dependency injection support (service locator, IoC containers)
+ Simplified system configuration via fluent DSL (client, cluster, embedded, azure)
+ Convenient unit testing kit (stubs, mocks, expectations)
+ Reactive Extensions (RX) support (client-side observers)
+ Frictionless exception handling (automatic unwrapping of AggregateException)
+ Higher-order catch-all function (AOP)
+ Support for non-uniform message handlers (noise reduction for C#)
+ Message handler auto-wiring (based on simple conventions)
+ Lambda-based message handlers (C#)
+ Reentrant messages
+ Extensible actor prototypes [PLANNED]
+ Automatic actor deactivation (configurable on per-type basis)

### Add-ons

##### Goodies

| ID | Link | Description
| ------- |:----:| ---------- |
| Orleankka.FSharp | [![NuGet](https://img.shields.io/nuget/v/Orleankka.FSharp.svg?style=flat)](https://www.nuget.org/packages/Orleankka.FSharp/) | Special api for F#
| Orleankka.TestKit | [![NuGet](https://img.shields.io/nuget/v/Orleankka.TestKit.svg?style=flat)](https://www.nuget.org/packages/Orleankka.TestKit/) | Unit testing kit
| Orleankka.Azure | [![NuGet](https://img.shields.io/nuget/v/Orleankka.Azure.svg?style=flat)](https://www.nuget.org/packages/Orleankka.Azure/) | Fluent configuration for Azure

##### Serialization

| ID | Link | Description
| ------- |:----:| ---------- |
| .NET binary | [Built-in] | Default. Standard binary seriailization. See `BinarySerializer` 
| Orleans native | [Built-in] | Orleans native codegened serialization. See `NativeSerializer` 
| JSON | [[NuGet](https://www.nuget.org/packages/Orleankka.Serialization.JSON)] | Newtonsoft.JSON serialization
| Bond | [PLANNED] | Microsoft Bond
| ProtoBuf | [PLANNED] | Google Protocol Buffers

##### DI

| ID | Link | Description
| ------- |:----:| ---------- |
| Service Locator | [Built-in] | Via `Bootstrapper`. See example [here](Source/Demo.App) 
| Unity   | [PLANNED] | Microsoft Unity IoC container
| NInject | [PLANNED] | NInject IoC container
| Autofac | [PLANNED] | Autofac IoC container

### How to install

To install Orleankka via NuGet, run this command in NuGet package manager console:

	PM> Install-Package Orleankka

Check out "Getting started" [guide](https://github.com/yevhen/Orleankka/wiki/Getting-Started-%28C%23%29) ([F#](https://github.com/yevhen/Orleankka/wiki/Getting-Started-%28F%23%29)).

### Documentation

Complete documentation could be found on [wiki](https://github.com/yevhen/Orleankka/wiki).

## Contributing

Any bug-fix pull request goes without a saying. For new features or modifications, please first create an issue, so we can discuss it before any effort is made. Add-ons, like new serialization protocols, DI container support, etc - are highly welcome!

## License

Apache 2 License
