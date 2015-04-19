using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;

using Orleankka;
using Orleankka.Core;

using Orleans.Serialization;

namespace Example.Serialization.JSON
{
    public class JsonSerializer : IMessageSerializer
    {
        JsonSerializerSettings settings;

        void IMessageSerializer.Init(Assembly[] assemblies, IDictionary<string, string> properties)
        {
            settings = new JsonSerializerSettings
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
                Converters = {new RefConverter()}
            };
        }

        void IMessageSerializer.Serialize(object message, BinaryTokenStreamWriter stream)
        {
            var json  = JsonConvert.SerializeObject(message, Formatting.None, settings);
            var bytes = Encoding.Default.GetBytes(json);
            SerializationManager.SerializeInner(bytes, stream, typeof(byte[]));
        }

        object IMessageSerializer.Deserialize(BinaryTokenStreamReader stream)
        {
            var bytes = (byte[]) SerializationManager.DeserializeInner(typeof(byte[]), stream);
            var json  = Encoding.Default.GetString(bytes);
            return JsonConvert.DeserializeObject(json, settings);
        }

        class RefConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(Ref).IsAssignableFrom(objectType);
            }

            public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
            {
                writer.WriteValue(((Ref)value).Serialize());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                return Ref.Deserialize((string)reader.Value); 
            }
        }
    }
}