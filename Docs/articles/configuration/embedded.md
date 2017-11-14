## Running both Client and Cluster in the same process
```cs
var system = ActorSystem.Configure()
    .Embedded()
    .Register(Assembly.GetExecutingAssembly())
    .Done();
```

The returned system will be an instance of the `IClientActorSystem` which you can use as a cluster client