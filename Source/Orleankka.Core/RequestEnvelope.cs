using System;
using System.Linq;

using Orleans.CodeGeneration;
using Orleans.Serialization;

namespace Orleankka.Core
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public class RequestEnvelope
    {
        public readonly string Target;
        public readonly object Message;

        internal RequestEnvelope(string target, object message)
        {
            Target = target;
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type unused)
        {
            var envelope = (RequestEnvelope) obj;
            stream.Write(envelope.Target);
            MessageEnvelope.Serializer.Serialize(envelope.Message, stream);
        }

        [DeserializerMethod]
        internal static object Deserialize(Type unused, BinaryTokenStreamReader stream)
        {
            var target = stream.ReadString();            
            var message = MessageEnvelope.Serializer.Deserialize(stream);
            return new RequestEnvelope(target, message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
