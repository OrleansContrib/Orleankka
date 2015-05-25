module Controller

open Orleankka
open Orleankka.FSharp
open Rop

open System
open System.Net
open System.Web.Http
open System.Net.Http
open System.Threading.Tasks
open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type ActorMsg = {
   Message : string
}

type ActorController(router:ActorRouter.Router) =
   inherit ApiController()

   [<HttpPost>]
   [<Route("api/{actor}/{id}")>]
   member this.Post(actor:string, id:string, [<FromBody>] msg:ActorMsg) = task {         
         
         if (box msg = null) || String.IsNullOrEmpty(msg.Message)
            then raise(HttpResponseException(HttpStatusCode.BadRequest))         

         match router.Dispatch(actor, id, msg.Message) with
         | Success t -> return! t            
         | Failure f -> return f :> obj
      }
      