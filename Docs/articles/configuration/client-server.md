## Configuring your application as a Client
To configure your application as only the client, you can configure the runtime as follows:
```cs
    var config = new ClientConfiguration()
               .LoadFromEmbeddedResource(typeof (Program),
               "Client.xml");

    var system = ActorSystem.Configure()
                .Client()
                .From(config)
                .Register(typeof (ChatServer).Assembly)
                .Done();
```
## Configuring your application as a Cluster
```cs
    var config = new ClientConfiguration()
               .LoadFromEmbeddedResource(typeof (Program),
               "Cluster.xml");

    var system = ActorSystem.Configure()
                .Cluster()
                .From(config)
                .Register(assembly)
                .Done();
```