using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using Newtonsoft.Json;

using Orleans;
using Orleankka.Scenarios.Dynamic;

[assembly: BootstrapperTestAction]

namespace Orleankka.Scenarios.Dynamic
{
    public class BootstrapperTestActionAttribute : TestActionAttribute
    {
        public override void BeforeTest(TestDetails details)
        {
            Bootstrapper.Init();
        }
    }

    public class Bootstrapper : Orleankka.Bootstrapper
    {
        public override Task Init(IDictionary<string, string> properties)
        {
            Init(); return TaskDone.Done;
        }

        internal static void Init()
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
                writer.WriteValue(ActorSystem.Dynamic.ActorType.Serializer((ActorPath) value));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return ActorSystem.Dynamic.ActorType.Deserializer((string) reader.Value);
            }
        }
    }
}
