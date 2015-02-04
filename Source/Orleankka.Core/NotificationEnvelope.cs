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
        public readonly string Sender;
        public readonly object Message;

        internal NotificationEnvelope(string sender, object message)
        {
            Sender = sender;
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type expected)
        {
            var envelope = (NotificationEnvelope)obj;
            stream.Write(envelope.Sender);

            var bytes = MessageEnvelope.Serializer.Serialize(envelope.Message);
            SerializationManager.SerializeInner(bytes, stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var sender = stream.ReadString();

            var bytes = (byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream);
            var message = MessageEnvelope.Serializer.Deserialize(bytes);

            return new NotificationEnvelope(sender, message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
