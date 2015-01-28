using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Orleankka.Internal
{
    static class Message
    {
        static Message()
        {
            Serializer = obj =>
            {
                using (var ms = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(ms, obj);
                    return ms.ToArray();
                }
            };

            Deserializer = bytes =>
            {
                using (var ms = new MemoryStream(bytes))
                {
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(ms);
                }
            };
        }

        internal static Func<object, byte[]> Serializer;
        internal static Func<byte[], object> Deserializer;
    }
}
