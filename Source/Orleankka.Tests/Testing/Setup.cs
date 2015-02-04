using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using NUnit.Framework;

using Orleankka.Scenarios;
using Orleankka.Testing;

[assembly: Setup]

namespace Orleankka.Testing
{
    using Core;
    using Playground;

    public class SetupAttribute : TestActionAttribute
    {
        IActorSystem system;

        public override void BeforeTest(TestDetails details)
        {
            if (!details.IsSuite)
                return;

            system = ActorSystem.Configure()
                .Playground()
                .Register(typeof(TestActor).Assembly)
                .Serializer<JsonSerializer>()
                .Done();
        }

        public override void AfterTest(TestDetails details)
        {
            if (!details.IsSuite)
                return;

            system.Dispose();
        }
    }

    public class JsonSerializer : IMessageSerializer
    {
        public void Init(IDictionary<string, string> properties)
        {}

        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters =
            {
                new ActorRefConverter(), 
                new ObserverRefConverter()
            }
        };

        byte[] IMessageSerializer.Serialize(object message)
        {
            string data = JsonConvert.SerializeObject(message, Formatting.None, JsonSerializerSettings);
            return Encoding.Default.GetBytes(data);
        }

        object IMessageSerializer.Deserialize(byte[] bytes)
        {
            string data = Encoding.Default.GetString(bytes);
            return JsonConvert.DeserializeObject(data, JsonSerializerSettings);
        }

        class ActorRefConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(ActorRef) == objectType;
            }

            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                var @ref = (ActorRef) value;
                writer.WriteValue(@ref.Serialize());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                return ActorRef.Deserialize((string)reader.Value);
            }
        }

        class ObserverRefConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(ObserverRef) == objectType;
            }

            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                var @ref = (ObserverRef)value;
                writer.WriteValue(@ref.Serialize());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                return ObserverRef.Deserialize((string)reader.Value);
            }
        }
    }
}
