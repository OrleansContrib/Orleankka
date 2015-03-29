using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace Orleankka.Utility
{
    using Core;

    public class JsonSerializer : IMessageSerializer
    {
        public void Init(IDictionary<string, string> properties)
        {}

        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters = {new RefConverter()}
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

        class RefConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(ActorRef)        == objectType 
                       || typeof(ClientRef)    == objectType
                       || typeof(ObserverRef)  == objectType;
            }

            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                var client = value as ClientRef;
                if (client != null)
                {
                    writer.WriteValue("C__" + client.Serialize());
                    return;
                }
                
                var actor = (ActorRef) value;
                writer.WriteValue(actor.Serialize());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                var path = (string)reader.Value;

                if (path.StartsWith("C__"))
                    return ClientRef.Deserialize(path.Substring(3));

                return ActorRef.Deserialize(path);
            }
        }
    }
}