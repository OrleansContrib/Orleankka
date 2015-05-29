using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;
using Orleankka.Cluster;
using Orleankka.Services;

using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;

using Orleans;

using Streamstone;
using Microsoft.WindowsAzure.Storage.Table;

namespace Example
{
    public abstract class CqsActor : Actor
    {
        protected override void Define()
        {
            Reentrant(req => req is Query);
        }

        protected override Task<object> OnReceive(object message)
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
        readonly IActivationService activation;

        protected EventSourcedActor()
        {
            activation = new ActivationService(this);
        }

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

        int version = -1;

        protected override async Task OnActivate()
        {
            version = 0;

            var streamName = StreamName();

            var streamOpenResult = Stream.TryOpen(SS.Table, new Partition(streamName));


            if (streamOpenResult.Found)
            {
                var stream = streamOpenResult.Stream;
                var slice = default(StreamSlice<EventEntity>);
                var nextSliceStart = 1;

                do
                {
                    slice = await Stream.ReadAsync<EventEntity>(SS.Table, streamName, nextSliceStart, sliceSize: 10);

                    nextSliceStart = slice.NextEventNumber;

                    Replay(slice.Events);
                }
                while (!slice.IsEndOfStream);
            }
        }

        string StreamName()
        {
            return GetType().Name + "-" + Id;
        }

        void Replay(IEnumerable<EventEntity> events)
        {
            var deserialized = events.Select(x => DeserializeEvent(x)).ToArray();
            Apply(deserialized);
        }

        protected override async Task<object> HandleCommand(Command cmd)
        {
            var events = DispatchResult<IEnumerable<Orleankka.Meta.Event>>(cmd).ToArray();
            
            await Store(events);
            Apply(events);
            
            return events;
        }

        void Apply(IEnumerable<Orleankka.Meta.Event> events)
        {
            foreach (var @event in events)
                Apply(@event);
        }

        void Apply(object @event)
        {
            Dispatch(@event);
            version++;
        }

        async Task Store(ICollection<Orleankka.Meta.Event> events)
        {            
            if (events.Count == 0)
                return;

            var streamName = StreamName();
            var stream = default(Stream);

            var streamOpenResult = Stream.TryOpen(SS.Table, new Partition(streamName));

            if (streamOpenResult.Found)
            {
                stream = streamOpenResult.Stream;
            }
            else
            {
                stream = Stream.Provision(SS.Table, new Partition(streamName));
            }

            var serialized = events.Select(ToEvent).ToArray();
            
            try
            {
                await Stream.WriteAsync(SS.Table, stream,  serialized);
            }
            catch (ConcurrencyConflictException)
            {
                Console.WriteLine("Concurrency conflict on stream '{0}' detected", streamName);
                Console.WriteLine("Probably, second activation of actor '{0}' has been created", Self);
                Console.WriteLine("Deactivating duplicate activation '{0}' ... ", Self);

                activation.DeactivateOnIdle();
                throw new InvalidOperationException("Duplicate activation of actor '" + Self + "' detected");
            }
        }

        static Orleankka.Meta.Event DeserializeEvent(EventEntity @event)
        {
            var eventType = Type.GetType(@event.Type);
            Debug.Assert(eventType != null, "Couldn't load type '{0}'. Are you missing an assembly reference?", @event.Type);

            return (Orleankka.Meta.Event) JsonConvert.DeserializeObject(@event.Data, eventType, SerializerSettings);
        }

        static Streamstone.Event ToEvent(object @event)
        {
            var eventId = Guid.NewGuid().ToString("D");

            var data = new EventEntity
            {
                Id = eventId,
                Type = @event.GetType().FullName,
                Data = JsonConvert.SerializeObject(@event, SerializerSettings)
            };

            return new Streamstone.Event(eventId, data.Props());
        }

        protected override Task<object> HandleQuery(Query query)
        {
            return Task.FromResult(DispatchResult(query));
        }

        class EventEntity
        {
            public string Id   { get; set; }
            public string Type { get; set; }
            public string Data { get; set; }
        }
    }

    public static class SS
    {
        public static CloudTable Table
        {
            get; private set;
        }

        public class Bootstrap : Bootstrapper<Properties>
        {
            public override Task Run(Properties properties)
            {
                var client = CloudStorageAccount.Parse(properties.StorageAccount).CreateCloudTableClient();
                Table = client.GetTableReference(properties.TableName);
                return TaskDone.Done;
            }
        }

        [Serializable]
        public class Properties
        {
            public string StorageAccount;
            public string TableName;
        }
    }
}