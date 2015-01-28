using System;
using System.Linq;

using Orleans.CodeGeneration;
using Orleans.Serialization;

namespace Orleankka.Internal
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public class Request
    {
        public readonly ActorPath Target;
        public readonly object Message;

        internal Request(ActorPath target, object message)
        {
            Target = target;
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type expected)
        {
            var request = (Request) obj;
            
            SerializationManager.SerializeInner(request.Target, stream, typeof(ActorPath));
            SerializationManager.SerializeInner(Internal.Message.Serializer(request.Message), stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var target = (ActorPath)SerializationManager.DeserializeInner(typeof(ActorPath), stream);
            var message = Internal.Message.Deserializer((byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream));

            return new Request(target, message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
