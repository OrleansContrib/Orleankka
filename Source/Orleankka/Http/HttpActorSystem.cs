using System;
using System.Net.Http;
using System.Text.Json;

namespace Orleankka.Http
{
    public class HttpActorSystem : IActorSystem
    {
        readonly ActorRouteMapper mapper;
        readonly HttpClient client;
        readonly JsonSerializerOptions serializer;
        readonly IActorRefMiddleware middleware;

        public HttpActorSystem(HttpClient client, JsonSerializerOptions serializer, ActorRouteMapper mapper, IActorRefMiddleware middleware = null)
        {
            if (!client.BaseAddress.AbsoluteUri.EndsWith("/"))
                throw new InvalidOperationException("The base address should end with /");

            this.mapper = mapper;
            this.client = client;
            this.serializer = serializer;
            this.middleware = middleware ?? DefaultActorRefMiddleware.Instance;
        }

        public ActorRef ActorOf(ActorPath path)
        {
            var endpoint = HttpActorEndpoint.From(client, serializer, mapper, path);
            return new ActorRef(path, endpoint, middleware);
        }

        public StreamRef<TItem> StreamOf<TItem>(StreamPath path) => throw new NotImplementedException();
        public ClientRef ClientOf(string path) => throw new NotImplementedException();
    }
}