using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

using NUnit.Framework;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Orleans;

namespace Orleankka.Features.Http_rpc
{
    namespace Request_response
    {
        using Http;
        using Http.AspNetCore;
        using Testing;

        [Serializable]
        public class SetText : ActorMessage<IAutoMappedActor>
        {
            public string Text { get; set; }
        }

        [Serializable]
        public class GetText : ActorMessage<IAutoMappedActor, GetText.Result>
        {
            [Serializable]
            public class Result
            {
                public string Text { get; set; }
            }
        }

        public interface IHandMappedActor : IActorGrain, IGrainWithStringKey
        { }

        public class HandMappedActor : DispatchActorGrain, IHandMappedActor
        {
            string text = "";

            public void On(SetText cmd) => text = cmd.Text;
            public GetText.Result On(GetText q) => new GetText.Result {Text = text};
        }

        public interface IAutoMappedActor : IActorGrain, IGrainWithStringKey
        { }

        public class AutoMappedActor : DispatchActorGrain, IAutoMappedActor
        {
            string text = "";

            public void On(SetText cmd) => text = cmd.Text;
            public GetText.Result On(GetText q) => new GetText.Result {Text = text};
        }

        [TestFixture]
        [RequiresSilo]
        public class Tests
        {
            const string prefix = "actors";
            readonly Uri baseAddress = new Uri($"http://localhost:9090/{prefix}/");

            readonly JsonSerializerOptions serializer = new JsonSerializerOptions 
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
                Converters = {new JsonStringEnumConverter()}
            };

            IHost host;
            IActorSystem system;
            ActorRouteMapper mapper;
            HttpActorSystem httpSystem;
            HttpClient httpClient;

            ActorRef handMappedActor;
            ActorRef<IAutoMappedActor> autoMappedActor;

            [SetUp]
            public async Task SetUp()
            {
                var handMapped = new ActorRouteMapping(typeof(IHandMappedActor), "TestActor");
                handMapped.Register(typeof(SetText), "SetText");
                handMapped.Register(typeof(GetText), "GetText", typeof(GetText.Result));

                var autoMapped = ActorRouteMapping.From(typeof(IAutoMappedActor));
                
                mapper = new ActorRouteMapper();
                mapper.Register(handMapped);
                mapper.Register(autoMapped);
                
                system = TestActorSystem.Instance;
                
                httpClient = new HttpClient {BaseAddress = baseAddress};
                httpSystem = new HttpActorSystem(httpClient, serializer, mapper);

                var builder = Host.CreateDefaultBuilder();
                host = builder.ConfigureWebHostDefaults(web =>
                {
                    web.UseUrls("http://*:9090");
                    web.ConfigureServices(services =>
                    {
                        services.AddSingleton(system);
                        services.AddSingleton(serializer);
                        services.AddSingleton(mapper);
                    });
                    web.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(e => e.MapActors(prefix));
                    });
                })
                .Build();

                await host.StartAsync();
                
                handMappedActor = httpSystem.ActorOf<IHandMappedActor>(Guid.NewGuid().ToString());
                autoMappedActor = httpSystem.TypedActorOf<IAutoMappedActor>(Guid.NewGuid().ToString());
            }

            [TearDown]
            public async Task TearDown()
            {
                await host.StopAsync();
                host.Dispose();
            }

            [Test]
            public async Task Hand_mapped()
            {
                const string text = "Hello world!";
                await handMappedActor.Tell(new SetText {Text = text});

                var response = await handMappedActor.Ask<GetText.Result>(new GetText());
                Assert.AreEqual(text, response.Text);
            }

            [Test]
            public async Task Auto_mapped()
            {
                const string text = "Look ma, no hands!";
                await autoMappedActor.Tell(new SetText {Text = text});

                var response = await autoMappedActor.Ask(new GetText());
                Assert.AreEqual(text, response.Text);
            }

            [Test]
            public async Task Http_get()
            {
                const string text = "Get it via GET!";
                await handMappedActor.Tell(new SetText {Text = text});

                var response = await GetHandMappedActor<GetText.Result>(handMappedActor, new GetText());
                Assert.AreEqual(text, response.Text);
            }

            async Task<TResponse> GetHandMappedActor<TResponse>(ActorRef actor, object message)
            {
                var path = $"TestActor/{actor.Path.Id}/{message.GetType().Name}";
                return (TResponse) await SendJson(path, result: typeof(TResponse));
            }

            async Task<object> SendJson(string path, object content = null, Type result = null)
            {
                var request = new HttpRequestMessage(content != null
                    ? HttpMethod.Post
                    : HttpMethod.Get, path);

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (content != null)
                    request.Content = new StringContent(JsonSerializer.Serialize(content, serializer));

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Request failed with {(int) response.StatusCode} code. See error below:\n{responseBody}");

                return !string.IsNullOrWhiteSpace(responseBody)
                    ? JsonSerializer.Deserialize(responseBody, result)
                    : default;
            }
        }
    }
}