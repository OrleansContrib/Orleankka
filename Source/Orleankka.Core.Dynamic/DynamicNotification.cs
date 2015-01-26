using System;
using System.Linq;

using Orleans.CodeGeneration;
using Orleans.Serialization;

namespace Orleankka.Dynamic.Internal
{
    /// <summary> 
    /// FOR INTERNAL USE ONLY! 
    /// </summary>
    public class DynamicNotification
    {
        public readonly ActorPath Source;
        public readonly object Message;

        internal DynamicNotification(ActorPath source, object message)
        {
            Source = source;
            Message = message;
        }

        [SerializerMethod]
        internal static void Serialize(object obj, BinaryTokenStreamWriter stream, Type t)
        {
            var msg = (DynamicNotification)obj;

            SerializationManager.SerializeInner(msg.Source, stream, typeof(ActorPath));
            SerializationManager.SerializeInner(DynamicMessage.Serializer(msg.Message), stream, typeof(byte[]));
        }

        [DeserializerMethod]
        internal static object Deserialize(Type t, BinaryTokenStreamReader stream)
        {
            var source = (ActorPath)SerializationManager.DeserializeInner(typeof(ActorPath), stream);
            var message = DynamicMessage.Deserializer((byte[])SerializationManager.DeserializeInner(typeof(byte[]), stream));

            return new DynamicNotification(source, message);
        }

        [CopierMethod]
        internal static object DeepCopy(object original)
        {
            return original;
        }
    }
}
