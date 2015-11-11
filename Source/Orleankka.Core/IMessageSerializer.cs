using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

using Orleans.Serialization;

namespace Orleankka.Core
{
    public interface IMessageSerializer
    {
        void Init(Assembly[] assemblies, object properties);

        /// <summary>
        /// Serializes message to byte[]
        /// </summary>
        void Serialize(object message, BinaryTokenStreamWriter stream);

        /// <summary>
        /// Deserializes byte[] back to message
        /// </summary>
        object Deserialize(BinaryTokenStreamReader stream);
    }

    public abstract class MessageSerializer<TProperties> : IMessageSerializer
    {
        void IMessageSerializer.Init(Assembly[] assemblies, object properties)
        {
            Init(assemblies, (TProperties)properties);
        }

        public abstract void Init(Assembly[] assemblies, TProperties properties);
        public abstract void Serialize(object message, BinaryTokenStreamWriter stream);
        public abstract object Deserialize(BinaryTokenStreamReader stream);
    }

    public abstract class MessageSerializer : MessageSerializer<object>
    {
        public override void Init(Assembly[] assemblies, object properties) {}
    }

    public sealed class BinarySerializer : MessageSerializer
    {
        public override void Serialize(object message, BinaryTokenStreamWriter stream)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, message);
                SerializationManager.SerializeInner(ms.ToArray(), stream, typeof(byte[]));
            }
        }

        public override object Deserialize(BinaryTokenStreamReader stream)
        {
            var bytes = (byte[]) SerializationManager.DeserializeInner(typeof(byte[]), stream);

            using (var ms = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
        }
    }

    public sealed class NativeSerializer : MessageSerializer
    {
        public override void Serialize(object message, BinaryTokenStreamWriter stream)
        {
            SerializationManager.SerializeInner(message, stream, null);
        }

        public override object Deserialize(BinaryTokenStreamReader stream)
        {
            return SerializationManager.DeserializeInner(null, stream);
        }
    }
}
