using System;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Orleans.Runtime;

namespace Orleankka.Testing
{
    using System.Text.Json;

    using JsonSerializer = Newtonsoft.Json.JsonSerializer;

    public class ObserverRefConverter : JsonConverter
    {
        readonly ClientRefConverter clientRefConverter;
        readonly ActorRefConverter actorRefConverter;

        public ObserverRefConverter(IActorSystem system)
        {
            clientRefConverter = new ClientRefConverter(system);
            actorRefConverter = new ActorRefConverter(system);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ObserverRef) == objectType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var type = obj["type"].Value<string>();

            return type switch {
                "cref" => clientRefConverter.Deserialize(obj),
                "aref" => actorRefConverter.Deserialize(obj),
                _ => throw new InvalidOperationException($"Unknown ref type: {type}")
            };
        }
    }

    public class ClientRefConverter : JsonConverter
    {
        readonly IActorSystem system;

        public ClientRefConverter(IActorSystem system)
        {
            this.system = system;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ClientRef).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("cref");
            writer.WritePropertyName("path");
            writer.WriteValue(value.ToString());
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            if (obj["type"].Value<string>() != "cref")
                throw new InvalidOperationException();
            return Deserialize(obj);
        }

        public object Deserialize(JObject obj) => system.ClientOf(obj["path"].Value<string>());
    }

    public class ActorRefConverter : JsonConverter
    {
        readonly IActorSystem system;

        public ActorRefConverter(IActorSystem system)
        {
            this.system = system;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ActorRef).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("aref");
            writer.WritePropertyName("path");
            writer.WriteValue(value.ToString());
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            if (obj["type"].Value<string>() != "aref")
                throw new InvalidOperationException();
            return Deserialize(obj);
        }

        public object Deserialize(JObject obj) => system.ActorOf(obj["path"].Value<string>());
    }

    public class TypedActorRefConverter : JsonConverter
    {
        readonly ActorRefConverter converter;

        public TypedActorRefConverter(IActorSystem system)
        {
            converter = new ActorRefConverter(system);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IStronglyTypedActorRef).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            converter.WriteJson(writer, value, serializer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var @ref = converter.ReadJson(reader, objectType, existingValue, serializer);
            var args = new object[]{@ref};
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            return Activator.CreateInstance(objectType, flags, null, args, null);
        }
    }
}