using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using NUnit.Framework;

using Orleans;
using Orleankka;

[assembly: DynamicActorsBootstrapper]

namespace Orleankka
{
    public class DynamicActorsBootstrapperAttribute : TestActionAttribute
    {
        public override void BeforeTest(TestDetails details)
        {
            DynamicActorsBootstrapper.Run();
        }
    }

    public class DynamicActorsBootstrapper : ActorSystemBootstrapper
    {
        public override Task Run(IDictionary<string, string> properties)
        {
            Run(); return TaskDone.Done;
        }

        internal static void Run()
        {
            ActorSystem.Dynamic.Serializer = Serialize;
            ActorSystem.Dynamic.Deserializer = Deserialize;
        }

        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters = {new ActorPathConverter()}
        };

        static byte[] Serialize(object obj)
        {
            string data = JsonConvert.SerializeObject(obj, Formatting.None, JsonSerializerSettings);
            return Encoding.Default.GetBytes(data);
        }

        static object Deserialize(byte[] bytes)
        {
            string data = Encoding.Default.GetString(bytes);
            return JsonConvert.DeserializeObject(data, JsonSerializerSettings);
        }

        class ActorPathConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(ActorPath) == objectType;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return ActorPath.Of((string)reader.Value);
            }
        }
    }
}
