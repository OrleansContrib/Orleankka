using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;
using Orleankka.Cluster;

using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace Example
{
    public abstract class CqsActor : Actor
    {
        public override Task<object> OnReceive(object message)
        {
            var cmd = message as Command;
            if (cmd != null)
                return HandleCommand(cmd);

            var query = message as Query;
            if (query != null)
                return HandleQuery(query);

            throw new InvalidOperationException("Unknown message type: " + message.GetType());
        }

        protected abstract Task<object> HandleCommand(Command cmd);
        protected abstract Task<object> HandleQuery(Query query);
    }

    public abstract class EventSourcedFsmActor : CqsActor
    {
        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            Culture = CultureInfo.GetCultureInfo("en-US"),
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            TypeNameHandling = TypeNameHandling.All,
            FloatParseHandling = FloatParseHandling.Decimal,
        };

        long version = ExpectedVersion.NoStream;

        protected EventSourcedFsmActor(string initial)
        {
            State = initial;
        }

        protected string State
        {
            get; set;
        }

        public override async Task OnActivate()
        {
            await Replay(StreamName());
            Behavior.Initial(State);
        }

        async Task Replay(string stream)
        {
            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;

            do
            {
                currentSlice = await ES.Connection
                    .ReadStreamEventsForwardAsync(stream, nextSliceStart, 256, false);

                if (currentSlice.Status == SliceReadStatus.StreamNotFound)
                    break;

                if (currentSlice.Status == SliceReadStatus.StreamDeleted)
                    throw new InvalidOperationException("Stream '" + stream + "' has beed unexpectedly deleted");

                nextSliceStart = currentSlice.NextEventNumber;
                await Replay(currentSlice.Events);
            }
            while (!currentSlice.IsEndOfStream);
        }

        string StreamName()
        {
            return GetType().Name + "-" + Id;
        }

        async Task Replay(IEnumerable<ResolvedEvent> events)
        {
            var deserialized = events.Select(x => DeserializeEvent(x.Event)).ToArray();
            await Apply(deserialized);
        }

        protected override async Task<object> HandleCommand(Command cmd)
        {
            var events = ((IEnumerable<Event>)await Behavior.HandleReceive(cmd)).ToArray();
            
            await Store(events);
            await Apply(events);

            if (Behavior.Current != State)
                await Behavior.Become(State);
            
            return events;
        }

        async Task Apply(IEnumerable<Event> events)
        {
            foreach (var @event in events)
                await Apply(@event);
        }

        async Task Apply(object @event)
        {
            await Dispatch(@event);
            version++;
        }

        async Task Store(ICollection<Event> events)
        {            
            if (events.Count == 0)
                return;

            var stream = StreamName();
            var serialized = events.Select(ToEventData).ToArray();

            try
            {
                await ES.Connection.AppendToStreamAsync(stream, version, serialized);
            }
            catch (WrongExpectedVersionException)
            {
                Console.WriteLine("Concurrency conflict on stream '{0}' detected", StreamName());
                Console.WriteLine("Probably, second activation of actor '{0}' has been created", Self);
                Console.WriteLine("Deactivating duplicate activation '{0}' ... ", Self);

                Activation.DeactivateOnIdle();
                throw new InvalidOperationException("Duplicate activation of actor '" + Self + "' detected");
            }
        }

        static Event DeserializeEvent(RecordedEvent @event)
        {
            var eventType = Type.GetType(@event.EventType);
            Debug.Assert(eventType != null, "Couldn't load type '{0}'. Are you missing an assembly reference?", @event.EventType);

            var json = Encoding.UTF8.GetString(@event.Data);
            return (Event) JsonConvert.DeserializeObject(json, eventType, SerializerSettings);
        }

        static EventData ToEventData(object processedEvent)
        {
            return ToEventData(Guid.NewGuid(), processedEvent, new Dictionary<string, object>());
        }

        static EventData ToEventData(Guid eventId, object evnt, IDictionary<string, object> headers)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt, SerializerSettings));
            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(headers, SerializerSettings));

            var eventTypeName = evnt.GetType().AssemblyQualifiedName;
            return new EventData(eventId, eventTypeName, true, data, metadata);
        }

        protected override Task<object> HandleQuery(Query query) => 
            Behavior.HandleReceive(query);
        
        protected void OnReceive<TCommand>(Func<TCommand, Event> handler) => 
            Behavior.OnReceive<TCommand, IEnumerable<Event>>(cmd => new[] {handler(cmd)});
    }

    public static class ES
    {
        public static IEventStoreConnection Connection
        {
            get; private set;
        }

        public class Bootstrap
        {
            public static async Task Run()
            {
                Connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
                await Connection.ConnectAsync();
            }
        }
    }
}