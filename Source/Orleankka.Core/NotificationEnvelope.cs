using System;
using System.Linq;

using Orleans.CodeGeneration;
using Orleans.Serialization;

namespace Orleankka.Core
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public sealed class NotificationEnvelope
    {
        public readonly object Message;

        internal NotificationEnvelope(object message)
        {
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type expected)
        {
            var envelope = (NotificationEnvelope)obj;
            var bytes = MessageEnvelope.Serializer.Serialize(envelope.Message);
            SerializationManager.SerializeInner(bytes, stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var bytes = (byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream);
            var message = MessageEnvelope.Serializer.Deserialize(bytes);
            return new NotificationEnvelope(message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
