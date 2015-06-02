namespace Orleankka.Http

open Orleankka.FSharp
open System
open System.Net
open System.Web.Http
open System.ComponentModel.DataAnnotations

type ActorController(router:ActorRouter.Router) =
   inherit ApiController()

   [<HttpPost>]
   [<Route("api/{actor}/{id}/{messagename}")>]
   member this.Post(actor:string, id:string, messagename:string, [<FromBody>] msgBody) = task {

      if not this.ModelState.IsValid
         then raise(HttpResponseException(HttpStatusCode.BadRequest))

      let httpPath = HttpRoute.createHttpPath(actor, id, messagename)

      match router.Dispatch(httpPath, msgBody) with
      | Some r -> return! r
      | None   -> ArgumentException() |> raise
   }
      