using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Orleankka.Internal
{
    static class Payload
    {
        static Payload()
        {
            Serialize = obj =>
            {
                using (var ms = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(ms, obj);
                    return ms.ToArray();
                }
            };

            Deserialize = bytes =>
            {
                using (var ms = new MemoryStream(bytes))
                {
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(ms);
                }
            };
        }

        internal static Func<object, byte[]> Serialize;
        internal static Func<byte[], object> Deserialize;
    }
}
