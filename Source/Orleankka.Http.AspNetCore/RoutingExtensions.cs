using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Orleankka.Http.AspNetCore
{
    public static class RoutingExtensions
    {
        public static void MapActors(this IEndpointRouteBuilder routes, string prefix = "")
        {
            var services = routes.ServiceProvider;
            var serializer = services.GetRequiredService<JsonSerializerOptions>();
            var system = services.GetRequiredService<IActorSystem>();
            var mapper = services.GetRequiredService<ActorRouteMapper>();
            MapActors(routes, system, serializer, mapper, prefix);
        }

        public static void MapActors(
            this IEndpointRouteBuilder routes, 
            IActorSystem system, 
            JsonSerializerOptions serializer, 
            ActorRouteMapper mapper, 
            string prefix = "")
        {
            routes.Map($"{prefix}/{{actor}}/{{id}}/{{message}}", async context =>
            {
                try
                {
                    await ProcessRequest();
                }
                catch (ActorRouteException ex)
                {
                    await RespondNotFound(ex.Message);
                }
                catch (Exception ex)
                {
                    await RespondError(ex.Message);
                }

                async Task ProcessRequest()
                {
                    var actor = FindActor();
                    var message = FindMessage(actor);

                    var request = await ReadRequest(message);
                    var response = await Send(actor, request);

                    if (response != null)
                        await WriteResponse(response);
                }

                string ActorRouteValue() => context.Request.RouteValues["actor"].ToString();
                string IdRouteValue() => context.Request.RouteValues["id"].ToString();
                string MessageRouteValue() => context.Request.RouteValues["message"].ToString();

                ActorRouteMapping FindActor()
                {
                    var actor = mapper.FindByRoute(ActorRouteValue());
                    return actor ?? throw new ActorRouteException($"Can't find actor with key: {ActorRouteValue()}");
                }

                MessageRouteMapping FindMessage(ActorRouteMapping actor)
                {
                    var message = actor.Find(MessageRouteValue());
                    return message ?? throw new ActorRouteException($"Can't find message '{MessageRouteValue()}' for actor '{ActorRouteValue()}'");
                }

                async Task RespondNotFound(string error)
                {
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    await context.Response.WriteAsync(error);
                }

                async Task RespondError(string error)
                {
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync(error);
                }

                async Task<object> Send(ActorRouteMapping route, object message)
                {
                    var actor = system.ActorOf(route.Interface, IdRouteValue());
                    return await actor.Ask<object>(message);
                }

                async Task<object> ReadRequest(MessageRouteMapping message)
                {
                    if (context.Request.Method == HttpMethod.Get.ToString())
                        return Activator.CreateInstance(message.Request);

                    if (context.Request.Method == HttpMethod.Post.ToString())
                        return await Deserialize(context.Request.BodyReader, message.Request, context.RequestAborted);

                    throw new ActorRouteException("Unsupported http verb: " + context.Request.Method);
                }

                async ValueTask WriteResponse(object result)
                {
                    await Serialize(result, context.Response.BodyWriter);
                }
            });

            async ValueTask<object> Deserialize(PipeReader reader, Type type, CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var frame = await reader.ReadAsync(cancellationToken);
                    var buffer = frame.Buffer;

                    var message = JsonSerializer.Deserialize(buffer.FirstSpan, type, serializer);
                    reader.AdvanceTo(buffer.Start, buffer.End);

                    if (frame.IsCompleted)
                        return message;
                }

                return null;
            }

            async ValueTask Serialize(object obj, PipeWriter writer)
            {
                await JsonSerializer.SerializeAsync(writer.AsStream(), obj, obj.GetType(), serializer);
            }
        }

        class ActorRouteException : Exception
        {
            public ActorRouteException(string message)
                : base(message)
            {}
        }
    }
}
