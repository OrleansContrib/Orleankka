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
    public abstract class EventSourcedActor : ActorGrain
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

        protected override async Task<object> OnReceive(object message)
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
                    return await base.OnReceive(message);
            }
        }

        async Task Load()
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

                Replay(slice.Events);
            }
            while (!slice.IsEndOfStream);
        }

        string StreamName()
        {
            return GetType().Name + "-" + Id;
        }

        void Replay(IEnumerable<EventEntity> events)
        {
            var deserialized = events.Select(DeserializeEvent).ToArray();
            Apply(deserialized);
        }

        Task<object> HandleQuery(Query query) => Result((dynamic)this).Handle((dynamic)query);

        async Task<object> HandleCommand(Command cmd)
        {
            var events = ((IEnumerable<Event>)((dynamic)this).Handle((dynamic)cmd)).ToArray();
            
            await Store(events);
            Apply(events);
            
            return events;
        }

        void Apply(IEnumerable<object> events)
        {
            foreach (var @event in events)
                ((dynamic)this).On((dynamic)@event);
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
    }

    public static class SS
    {
        public static CloudTable Table
        {
            get; set;
        }
    }
}