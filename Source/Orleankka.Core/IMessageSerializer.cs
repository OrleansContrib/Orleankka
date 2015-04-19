using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

using Orleans.Serialization;

namespace Orleankka.Core
{
    public interface IMessageSerializer
    {
        void Init(Assembly[] assemblies, IDictionary<string, string> properties);

        /// <summary>
        /// Serializes message to byte[]
        /// </summary>
        void Serialize(object message, BinaryTokenStreamWriter stream);

        /// <summary>
        /// Deserializes byte[] back to message
        /// </summary>
        object Deserialize(BinaryTokenStreamReader stream);
    }

    public class BinarySerializer : IMessageSerializer
    {
        void IMessageSerializer.Init(Assembly[] assemblies, IDictionary<string, string> properties)
        {}

        void IMessageSerializer.Serialize(object message, BinaryTokenStreamWriter stream)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, message);
                SerializationManager.SerializeInner(ms.ToArray(), stream, typeof(byte[]));
            }
        }

        object IMessageSerializer.Deserialize(BinaryTokenStreamReader stream)
        {
            var bytes = (byte[]) SerializationManager.DeserializeInner(typeof(byte[]), stream);

            using (var ms = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
        }
    }

    public class NativeSerializer : IMessageSerializer
    {
        void IMessageSerializer.Init(Assembly[] assemblies, IDictionary<string, string> properties)
        {}

        void IMessageSerializer.Serialize(object message, BinaryTokenStreamWriter stream)
        {
            SerializationManager.SerializeInner(message, stream, null);
        }

        object IMessageSerializer.Deserialize(BinaryTokenStreamReader stream)
        {
            return SerializationManager.DeserializeInner(null, stream);
        }
    }
}
