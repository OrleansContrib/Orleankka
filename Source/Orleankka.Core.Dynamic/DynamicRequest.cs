using System;
using System.Linq;

using Orleans.CodeGeneration;
using Orleans.Serialization;

namespace Orleankka.Dynamic.Internal
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public class DynamicRequest
    {
        public readonly ActorPath Target;
        public readonly object Message;

        internal DynamicRequest(ActorPath target, object message)
        {
            Target = target;
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type expected)
        {
            var request = (DynamicRequest) obj;
            
            SerializationManager.SerializeInner(request.Target, stream, typeof(ActorPath));
            SerializationManager.SerializeInner(DynamicMessage.Serializer(request.Message), stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var target = (ActorPath)SerializationManager.DeserializeInner(typeof(ActorPath), stream);
            var message = DynamicMessage.Deserializer((byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream));

            return new DynamicRequest(target, message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
