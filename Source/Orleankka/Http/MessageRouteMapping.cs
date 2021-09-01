using System;

namespace Orleankka.Http
{
    public class MessageRouteMapping
    {
        public readonly string Route;
        public readonly Type Request;
        public readonly Type Result;

        public MessageRouteMapping(Type request, string route, Type result = null)
        {
            Request = request;
            Route = route;
            Result = result;
        }
    }
}