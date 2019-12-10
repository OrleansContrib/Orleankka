using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka.Cluster
{
    using Utility;
    
    class ClusterActorSystem : ActorSystem
    {
        readonly Dictionary<Type, ActorGrainImplementation> implementations = 
             new Dictionary<Type, ActorGrainImplementation>();

        readonly IActorMiddleware actorMiddleware;

        internal ClusterActorSystem(
            Assembly[] assemblies,
            IServiceProvider serviceProvider,
            IActorRefMiddleware actorRefMiddleware = null, 
            IActorMiddleware actorMiddleware = null)
            : base(assemblies, serviceProvider, actorRefMiddleware)
        {
            this.actorMiddleware = actorMiddleware ?? DefaultActorMiddleware.Instance;
            Register(assemblies);
        }

        void Register(IEnumerable<Assembly> assemblies)
        {
            foreach (var each in assemblies.SelectMany(x => x.GetTypes().Where(IsActorGrain)))
            {
                var implementation = new ActorGrainImplementation(each, actorMiddleware);
                implementations.Add(each, implementation);
            }

            bool IsActorGrain(Type type) => !type.IsAbstract && typeof(ActorGrain).IsAssignableFrom(type);
        }

        internal ActorGrainImplementation ImplementationOf(Type grain) => implementations.Find(grain);        
    }
}