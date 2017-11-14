# How to configure

All of the actor system builders support passing native `ClientConfiguration` and/or `ClusterConfiguration`. Create them using programmatic api of by loading external configuration from XML file (you can also mix). 

## Load from an Embedded Resource
You can load your Orleans configuration from an XML file embedded in an assembly.

Add an XML file to your project and set the **Build Action** to `Embedded Resource` and set **Copy to Output Directory** to `Do not copy`. Use provided extension method to load:
```cs
    var config = new ClientConfiguration()
               .LoadFromEmbeddedResource(typeof (Program),
               "Client.xml");
```
Then you can futher modify it using programmatic api.