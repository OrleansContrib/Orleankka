using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

using Streamstone;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Example
{
    public abstract class CqsActor : ActorGrain
    {
        public override Task<object> Receive(object message)
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

    public abstract class EventSourcedActor : CqsActor
    {
        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            Culture = CultureInfo.GetCultureInfo("en-US"),
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            TypeNameHandling = TypeNameHandling.None,
            FloatParseHandling = FloatParseHandling.Decimal,
            Formatting = Formatting.None
        };

        Stream stream;

        async Task On(Activate _)
        {
            var partition = new Partition(SS.Table, StreamName());

            var existent = await Stream.TryOpenAsync(partition);
            if (!existent.Found)
            {
                stream = new Stream(partition);
                return;
            }

            stream = existent.Stream;
            StreamSlice<EventEntity> slice;
            var nextSliceStart = 1;

            do
            {
                slice = await Stream.ReadAsync<EventEntity>(partition, nextSliceStart);
                
                nextSliceStart = slice.HasEvents
                    ? slice.Events.Last().Version + 1
                    : -1;

                await Replay(slice.Events);
            }
            while (!slice.IsEndOfStream);
        }

        string StreamName()
        {
            return GetType().Name + "-" + Id;
        }

        async Task Replay(IEnumerable<EventEntity> events)
        {
            var deserialized = events.Select(DeserializeEvent).ToArray();
            await Apply(deserialized);
        }

        protected override async Task<object> HandleCommand(Command cmd)
        {
            var events = (await Dispatch<IEnumerable<object>>(cmd)).ToArray();
            
            await Store(events);
            await Apply(events);
            
            return events;
        }

        async Task Apply(IEnumerable<object> events)
        {
            foreach (var @event in events)
                await Dispatch(@event);
        }

        async Task Store(ICollection<object> events)
        {            
            if (events.Count == 0)
                return;

            var serialized = events.Select(ToEventData).ToArray();
            
            try
            {
                var result = await Stream.WriteAsync(stream, serialized);
                stream = result.Stream;
            }
            catch (ConcurrencyConflictException)
            {
                Console.WriteLine("Concurrency conflict on stream '{0}' detected", StreamName());
                Console.WriteLine("Probably, second activation of actor '{0}' has been created", Self);
                Console.WriteLine("Deactivating duplicate activation '{0}' ... ", Self);

                Activation.DeactivateOnIdle();
                throw new InvalidOperationException("Duplicate activation of actor '" + Self + "' detected");
            }
        }

        static object DeserializeEvent(EventEntity @event)
        {
            var eventType = Type.GetType(@event.Type);
            
            Debug.Assert(eventType != null, 
                "Couldn't load type '{0}'. Are you missing an assembly reference?", @event.Type);

            return JsonConvert.DeserializeObject(@event.Data, eventType, SerializerSettings);
        }

        static EventData ToEventData(object @event)
        {
            var id = Guid.NewGuid().ToString("D");

            var properties = new EventEntity
            {
                Id = id,
                Type = @event.GetType().FullName,
                Data = JsonConvert.SerializeObject(@event, SerializerSettings)
            };

            return new EventData(EventId.From(id), EventProperties.From(properties));
        }
        
        class EventEntity
        {
            public string Id   { get; set; }
            public string Type { get; set; }
            public string Data { get; set; }
            public int Version { get; set; }
        }

        protected override Task<object> HandleQuery(Query query)
        {
            return Dispatch(query);
        }
    }

    public static class SS
    {
        public static CloudTable Table
        {
            get; set;
        }
    }
}