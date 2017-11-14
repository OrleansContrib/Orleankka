## Playing with cluster
Sometimes you just have the need to spin up an integration environment. The Playground configurator will configure the embedded `ActorSystem` to have some default configuration.

```cs
var system = ActorSystem.Configure()
    .Playground()
    .Register(Assembly.GetExecutingAssembly())
    .Done();
```