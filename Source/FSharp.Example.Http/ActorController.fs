module Controller

open Orleankka.FSharp
open Rop

open System.Net
open System.Web.Http
open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type ActorMsg = {
   [<Required>] Message : string
}

type ActorController(router:ActorRouter.Router) =
   inherit ApiController()

   [<HttpPost>]
   [<Route("api/{actor}/{id}")>]
   member this.Post(actor:string, id:string, [<FromBody>] msg:ActorMsg) = task {         
         
         if not this.ModelState.IsValid
            then raise(HttpResponseException(HttpStatusCode.BadRequest))         

         match router.Dispatch(actor, id, msg.Message) with
         | Success t -> return! t            
         | Failure f -> return f :> obj
      }
      