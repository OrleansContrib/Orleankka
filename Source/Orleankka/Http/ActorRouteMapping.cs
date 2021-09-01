using System;
using System.Linq;
using System.Collections.Generic;

namespace Orleankka.Http
{
    public class ActorRouteMapping
    {
        public static ActorRouteMapping From(Type @interface)
        {
            var query = typeof(ActorMessage<,>);
            var command = typeof(ActorMessage<>);

            Type ActorMessageInterface(Type t) => t.GetInterfaces().FirstOrDefault(x => 
                x.IsGenericType &&
                (x.GetGenericTypeDefinition() == command || 
                x.GetGenericTypeDefinition() == query));

            Type ActorMessageResult(Type t)
            {
                var message = ActorMessageInterface(t);
                return message!.GetGenericTypeDefinition() == query 
                    ? message.GenericTypeArguments[1] 
                    : null;
            }

            bool IsActorMessage(Type t)
            {
                var message = ActorMessageInterface(t);
                return message != null && message.GenericTypeArguments[0] == @interface;
            }

            var mapping = new ActorRouteMapping(@interface, @interface.FullName);
            var messages = @interface.Assembly.GetTypes().Where(IsActorMessage);

            foreach (var message in messages)
                mapping.Register(message, message.FullName, ActorMessageResult(message));

            return mapping;
        }

        public readonly string Route;
        public readonly Type Interface;

        readonly Dictionary<string, MessageRouteMapping> messages = new Dictionary<string, MessageRouteMapping>();

        public ActorRouteMapping(Type @interface, string route)
        {
            Interface = @interface;
            Route = route;
        }

        public void Register(Type request, string route, Type result = null)
        {
            var mapping = new MessageRouteMapping(request, route, result);
            messages.Add(mapping.Route.ToLowerInvariant(), mapping);
        }

        public MessageRouteMapping Find(string route) => 
            messages.TryGetValue(route.ToLowerInvariant(), out var result) ? result : null;

        public MessageRouteMapping Find(Type request) => 
            messages.Values.FirstOrDefault(x => x.Request == request);
        
        public IEnumerable<MessageRouteMapping> Messages => messages.Values;
    }
}