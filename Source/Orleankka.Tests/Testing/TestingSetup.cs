using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using NUnit.Framework;

using Orleans;
using Orleans.Runtime.Configuration;

using Orleankka.Scenarios;
using Orleankka.Testing;

[assembly: TestSuiteSetup]

namespace Orleankka.Testing
{
    public class TestSuiteSetupAttribute : TestActionAttribute
    {
        IDisposable silo;

        public override void BeforeTest(TestDetails details)
        {
            if (!details.IsSuite)
                return;

            var serverConfig = new ServerConfiguration()
                .LoadFromEmbeddedResource(GetType(), "Orleans.Server.Configuration.xml");

            var clientConfig = new ClientConfiguration()
                .LoadFromEmbeddedResource(GetType(), "Orleans.Client.Configuration.xml");

            silo = new EmbeddedSilo()
                .With(serverConfig)
                .With(clientConfig)
                .Use<SerializationBootstrapper>()
                .Register(typeof(TestActor).Assembly)
                .Start();

            SerializationBootstrapper.Run();
        }

        public override void AfterTest(TestDetails details)
        {
            if (!details.IsSuite)
                return;

            silo.Dispose();
        }
    }

    public class SerializationBootstrapper : Bootstrapper
    {
        public override Task Run(IDictionary<string, string> properties)
        {
            Run();
            return TaskDone.Done;
        }

        public static void Run()
        {
            ActorSystem.Serializer = Serialize;
            ActorSystem.Deserializer = Deserialize;
        }

        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters = { new ActorPathConverter() }
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
                return ActorPath.From((string)reader.Value);
            }
        }
    }
}
