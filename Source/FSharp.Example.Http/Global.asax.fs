namespace Http

open Orleankka
open Orleankka.Playground

open System
open System.Web.Http
open System.Reflection
open Microsoft.Practices.Unity
open Newtonsoft.Json

type Global() =
   inherit System.Web.HttpApplication() 

   static member RegisterWebApi(config: HttpConfiguration) =
      
      let system = ActorSystem.Configure()
                              .Playground()
                              .Register(Assembly.GetExecutingAssembly())
                              .Done()

      let testActor = system.ActorOf<Actors.TestActor>("http_test")

      // configure actor routing
      let router = [(testActor, typedefof<Actors.HelloMessage>)]
                   |> Seq.map ActorRouter.mapToPath
                   |> ActorRouter.create JsonConvert.DeserializeObject

      config.MapHttpAttributeRoutes()

      // configure unity container
      let container = new UnityContainer()
      container.RegisterInstance<ActorRouter.Router>(router) |> ignore
      config.DependencyResolver <- new Resolver.UnityResolver(container)
      
      // configure serialization for json     
      let jsonFormatter = config.Formatters.JsonFormatter
      config.Formatters.Clear()
      config.Formatters.Add(jsonFormatter)
      config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()

    member x.Application_Start() =
        GlobalConfiguration.Configure(Action<_>(Global.RegisterWebApi))
