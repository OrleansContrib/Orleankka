using System;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

using NUnit.Framework;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Orleans;

namespace Orleankka.Features.Http_rpc
{
    namespace Request_response
    {
        using Meta;
        using Testing;

        [Serializable]
        public class SetText : Command
        {
            public string Text { get; set; }
        }

        [Serializable]
        public class GetText : Query<Result>
        {
            [Serializable]
            public class Result
            {
                public string Text { get; set; }
            }
        }

        public interface ITestActor : IActorGrain, IGrainWithStringKey
        {}

        public class TestActor : DispatchActorGrain, ITestActor
        {
            string text = "";

            public void On(SetText cmd) => text = cmd.Text;
            public GetText.Result On(GetText q) => new GetText.Result {Text = text};
        }

        [TestFixture]
        [RequiresSilo]
        public class Tests
        {
            IActorSystem system;
            IHost host;
            HttpClient http;

            readonly JsonSerializerOptions options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true, 
                AllowTrailingCommas = true,
                IgnoreNullValues = true,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            [SetUp]
            public async Task SetUp()
            {
                system = TestActorSystem.Instance;

                http = new HttpClient {BaseAddress = new Uri("http://localhost:9090")};
                
                var builder = Host.CreateDefaultBuilder();
                host = builder.ConfigureWebHostDefaults(web =>
                {
                  web.UseUrls("http://*:9090");
                  web.Configure(app =>
                  {
                      app.UseRouting();
                      app.UseEndpoints(endpoints =>
                      {
                          endpoints.MapPost("/grains/{grainKey}/{grainId}/{messageKey}", async context =>
                          {
                              var grainKey = context.Request.RouteValues["grainKey"].ToString();
                              var grainId = context.Request.RouteValues["grainId"].ToString();
                              var messageKey = context.Request.RouteValues["messageKey"].ToString();

                              var grainType = GrainType(grainKey);
                              var messageType = MessageType(grainKey, messageKey);

                              var actor = system.ActorOf(grainType, grainId);
                              var message = await Deserialize(context.Request.BodyReader, messageType, context.RequestAborted);

                              var result = await actor.Ask<object>(message);
                              if (result != null)
                                await Serialize(result, context.Response.BodyWriter);
                          });
                      });
                  });
                })
                .Build();

                await host.StartAsync();
            }

            async ValueTask<object> Deserialize(PipeReader reader, Type type, CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var frame = await reader.ReadAsync(cancellationToken);
                    var buffer = frame.Buffer;

                    var message = JsonSerializer.Deserialize(buffer.FirstSpan, type, options);
                    reader.AdvanceTo(buffer.Start, buffer.End);

                    if (frame.IsCompleted)
                        return message;
                }

                return null;
            }

            async ValueTask Serialize(object obj, PipeWriter writer)
            {
                await JsonSerializer.SerializeAsync(writer.AsStream(), obj, obj.GetType(), options);
            }


            Type GrainType(string grainKey)
            {
                return grainKey.ToLower() switch {
                    "testactor" => typeof(ITestActor),
                    _ => throw new Exception("meh")
                };
            }

            Type MessageType(string grainKey, string messageKey)
            {
                return messageKey switch {
                    nameof(GetText) => typeof(GetText),
                    nameof(SetText) => typeof(SetText),
                    _ => throw new Exception("meh")
                };

            }

            [TearDown]
            public async Task TearDown()
            {
                await host.StopAsync();
                host.Dispose();
            }

            [Test]
            public async Task Test_http()
            {
                const string text = "Hello world!";

                var actor = system.FreshActorOf<ITestActor>();
                await PostJson<object>($"/grains/TestActor/{actor.Path.Id}/{nameof(SetText)}", new SetText {Text = text});
                
                var response = await PostJson<GetText.Result>($"/grains/TestActor/{actor.Path.Id}/{nameof(GetText)}", new GetText());
                Assert.AreEqual(text, response.Text);
            }

            async Task<TResponse> PostJson<TResponse>(string path, object value)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(JsonSerializer.Serialize(value, options));

                var response = await http.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                return !string.IsNullOrWhiteSpace(responseBody)
                      ? JsonSerializer.Deserialize<TResponse>(responseBody)
                      : default;
            }
        }
    }
}