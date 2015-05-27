module Controller

open ActorRouter
open Orleankka.FSharp
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
   [<Route("api/{actor}/{id}/{messagetype}")>]
   member this.Post(actor:string, id:string, messagetype:string, [<FromBody>] msg:ActorMsg) = task {         

      if not this.ModelState.IsValid
         then raise(HttpResponseException(HttpStatusCode.BadRequest))         

      let path = ActorRouter.createPath(actor, id, messagetype)

      match router.Dispatch(path, msg.Message) with
      | Success t -> return! t            
      | Failure f -> return f :> obj
   }
      