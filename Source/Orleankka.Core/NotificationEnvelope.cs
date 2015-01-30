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
        public readonly ActorPath Sender;
        public readonly object Message;

        internal NotificationEnvelope(ActorPath sender, object message)
        {
            Sender = sender;
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type t)
        {
            var envelope = (NotificationEnvelope)obj;
            SerializationManager.SerializeInner(envelope.Sender, stream, typeof(ActorPath));
            SerializationManager.SerializeInner(MessageEnvelope.Serializer(envelope.Message), stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var sender  = (ActorPath)SerializationManager.DeserializeInner(typeof(ActorPath), stream);
            var message = MessageEnvelope.Deserializer((byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream));
            return new NotificationEnvelope(sender, message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
