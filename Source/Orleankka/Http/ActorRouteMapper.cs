using System.Linq;
using System.Collections.Generic;

namespace Orleankka.Http
{
    public class ActorRouteMapper
    {
        readonly Dictionary<string, ActorRouteMapping> actors = new Dictionary<string, ActorRouteMapping>();
        
        public void Register(ActorRouteMapping mapping) =>
            actors.Add(mapping.Route.ToLowerInvariant(), mapping);

        public ActorRouteMapping FindByRoute(string route) =>
            actors.TryGetValue(route.ToLowerInvariant(), out var mapping) ? mapping : null;

        public ActorRouteMapping FindByInterface(string fullName) =>
            actors.Values.FirstOrDefault(x => x.Interface.FullName == fullName);

        public IEnumerable<ActorRouteMapping> Actors => actors.Values;
    }
}