open Suave                 
open Suave.Web
open Suave.Http
open Suave.Http.Successful
open Suave.Http.RequestErrors
open Suave.Http.Applicatives
open Suave.Types
open Suave.Utils

open Orleankka
open Orleankka.Http
open Orleankka.Playground

open System.Reflection            
open Newtonsoft.Json


[<EntryPoint>]
let main argv = 
  
  printfn "Running demo. Booting cluster might take some time ...\n"

  // configure actor system
  use system = ActorSystem.Configure()
                          .Playground()
                          .Register(Assembly.GetExecutingAssembly())
                          .Done()
  
  let testActor = system.ActorOf<Actors.TestActor>("http_test")


  // configure actor routing
  let router = [(MessageType.DU(typeof<Actors.HelloMessage>), testActor.Path)]
                |> Seq.collect HttpRoute.create
                |> ActorRouter.create JsonConvert.DeserializeObject


  let hasContentType (ctx:HttpContext) = async {
    match ctx.request.header "content-type" with         
    | Choice1Of2 v when v = ContentType.Orleankka -> 
           return Some ctx
    | _ -> return None
  }    

  // sends msg to actor 
  let sendMsg actorPath (ctx:HttpContext) = async {    
    
    let msgBody = ctx.request.rawForm |> UTF8.toString
        
    match router.Dispatch(actorPath, msgBody) with
    | Some t -> let! result = Async.AwaitTask t
                return! OK (result.ToString()) ctx
    | None   -> return! BAD_REQUEST "actor has not found, or message has invalid format" ctx  
  }  

  // configure Suave routing
  let app = POST >>= hasContentType >>= pathScan "/api/%s" (fun path -> request (fun req ctx -> sendMsg path ctx))  

  printfn "Finished booting cluster...\n"

  startWebServer defaultConfig app
  0 
