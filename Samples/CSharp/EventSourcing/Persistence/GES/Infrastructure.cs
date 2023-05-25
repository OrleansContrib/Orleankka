using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

using Newtonsoft.Json;

namespace Example
{
    using EventStore.Client;

    public abstract class EventSourcedActor : DispatchActorGrain
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

        StreamRevision revision = StreamRevision.None;

        public override async Task<object> Receive(object message)
        {
            switch (message)
            {
                case Activate _:
                    await Load();
                    return Done;

                case Command cmd:
                    return await HandleCommand(cmd);
                
                case Query query:
                    return await HandleQuery(query);

                default:
                    return await base.Receive(message);
            }
        }

        async Task Load()
        {
            var stream = StreamName();

            var nextSliceStart = StreamPosition.Start;
            do
            {
                var result = ES.Client.ReadStreamAsync(Direction.Forwards, stream, nextSliceStart, 256);
                
                var state = await result.ReadState;
                if (state == ReadState.StreamNotFound)
                    return;
                
                var events = await result.ToListAsync();
                nextSliceStart = result.LastStreamPosition.GetValueOrDefault(StreamPosition.End);

                Replay(events);
            } 
            while (nextSliceStart != StreamPosition.End);
        }

        string StreamName() => Id;

        void Replay(IEnumerable<ResolvedEvent> events)
        {
            var deserialized = events.Select(x => DeserializeEvent(x.Event)).ToArray();
            Apply(deserialized);
        }

        Task<object> HandleQuery(Query query) => Result(Dispatcher.DispatchResult(this, query));

        async Task<object> HandleCommand(Command cmd)
        {
            var events = Dispatcher.DispatchResult<IEnumerable<Event>>(this, cmd).ToArray();
            
            await Store(events);
            Apply(events);
            
            return events;
        }

        void Apply(IEnumerable<Event> events)
        {
            foreach (var @event in events)
                Apply(@event);
        }

        void Apply(object @event)
        {
            Dispatcher.Dispatch(this, @event);
            revision++;
        }

        async Task Store(ICollection<Event> events)
        {            
            if (events.Count == 0)
                return;

            var stream = StreamName();
            var serialized = events.Select(ToEventData).ToArray();

            try
            {
                await ES.Client.AppendToStreamAsync(stream, revision, serialized);
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

        static Event DeserializeEvent(EventRecord @event)
        {
            var eventType = Type.GetType(@event.EventType);
            Debug.Assert(eventType != null, "Couldn't load type '{0}'. Are you missing an assembly reference?", @event.EventType);

            var json = Encoding.UTF8.GetString(@event.Data.ToArray());
            return (Event) JsonConvert.DeserializeObject(json, eventType, SerializerSettings);
        }

        static EventData ToEventData(object processedEvent)
        {
            return ToEventData(Uuid.NewUuid(), processedEvent, new Dictionary<string, object>());
        }

        static EventData ToEventData(Uuid eventId, object @event, IDictionary<string, object> headers)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, SerializerSettings));
            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(headers, SerializerSettings));

            var eventTypeName = @event.GetType().AssemblyQualifiedName;
            return new EventData(eventId, eventTypeName, data, metadata);
        }
    }

    public static class ES
    {
        public static EventStoreClient Client
        {
            get; set;
        }
    }
}