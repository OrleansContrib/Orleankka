namespace Resolver

open System
open System.Web.Http.Dependencies
open Microsoft.Practices.Unity

type UnityResolver(container:IUnityContainer) =   
   interface IDependencyResolver with

      member this.BeginScope() = 
         this :> IDependencyScope
      
      member this.Dispose() = ()
               
      member this.GetService(serviceType:Type) = 
         try          
            container.Resolve(serviceType)
         with
         | :? ResolutionFailedException -> null
         
      member this.GetServices(serviceType:Type) = 
         try
            container.ResolveAll(serviceType)
         with
         | :? ResolutionFailedException -> Seq.empty
      