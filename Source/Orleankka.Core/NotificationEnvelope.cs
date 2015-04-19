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
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type unused)
        {
            var envelope = (NotificationEnvelope)obj;
            MessageEnvelope.Serializer.Serialize(envelope.Message, stream);            
        }

        [DeserializerMethod]
        internal static object Deserialize(Type unused, BinaryTokenStreamReader stream)
        {
            var message = MessageEnvelope.Serializer.Deserialize(stream);
            return new NotificationEnvelope(message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
