namespace Orleankka.Http

open Orleankka.FSharp
open System.Net
open System.Net.Http
open System.Web.Http

type ActorController(router:ActorRouter.Router) =
   inherit ApiController()   

   [<HttpPost>]
   [<Route("api/{actor}/{id}/{messagename}")>]
   member this.Post(actor:string, id:string, messagename:string, [<FromBody>] msgBody) = task {
      
      let httpPath = HttpRoute.createHttpPath(actor, id, messagename)      

      match router.Dispatch(httpPath, msgBody) with
      | Some r -> let! response = r
                  return this.Request.CreateResponse(HttpStatusCode.OK, response)
      
      | None   -> return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "actor was not found by this path: " + httpPath)
   }
      