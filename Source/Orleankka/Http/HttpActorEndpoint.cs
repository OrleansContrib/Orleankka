using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka.Http
{
    class HttpActorEndpoint : IActorGrain
    {
        public static HttpActorEndpoint From(
            HttpClient client,
            JsonSerializerOptions serializer,
            ActorRouteMapper mapper,
            ActorPath path)
        {
            var mapping = mapper.FindByInterface(path.Interface);
            if (mapping == null)
                throw new Exception($"Can't find actor mapping for {path.Interface}");

            return new HttpActorEndpoint(client, serializer, mapping, path.Id);
        }

        readonly HttpClient client;
        readonly JsonSerializerOptions serializer;
        readonly ActorRouteMapping mapping;
        readonly string id;

        HttpActorEndpoint(HttpClient client, JsonSerializerOptions serializer, ActorRouteMapping mapping, string id)
        {
            this.client = client;
            this.serializer = serializer;
            this.mapping = mapping;
            this.id = id;
        }

        public async Task<object> ReceiveAsk(object message) => await Send(message);
        public async Task ReceiveTell(object message) => await Send(message, result: false);

        async Task<object> Send(object message, bool result = true)
        {
            var messageMapping = mapping.Find(message.GetType());

            if (messageMapping == null)
                throw new Exception($"Can't find message '{message.GetType()}' mapping for actor '{mapping.Route}'");

            if (result && messageMapping.Result == null)
                throw new Exception($"Message '{message.GetType()}' mapping for actor '{mapping.Route}' doesn't map return type");

            var path = $"{mapping.Route}/{id}/{messageMapping.Route}";
            return await SendJson(path, message, messageMapping.Result);
        }

        async Task<object> SendJson(string path, object content = null, Type result = null)
        {
            var request = new HttpRequestMessage(content != null
                ? HttpMethod.Post
                : HttpMethod.Get, path);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (content != null)
                request.Content = new StringContent(JsonSerializer.Serialize(content, serializer));

            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Request failed with {(int) response.StatusCode} code. See error below:\n{responseBody}");

            return !string.IsNullOrWhiteSpace(responseBody)
                ? JsonSerializer.Deserialize(responseBody, result)
                : default;
        }

        Task IActorGrain.ReceiveNotify(object message) => 
            throw new NotImplementedException();

        Task IRemindable.ReceiveReminder(string reminderName, TickStatus status) => 
            throw new NotImplementedException();
    }
}