using Orleans.Serialization;

namespace Orleankka.TestKit
{
    using Core;

    class OrleansSerialization
    {
        internal static void Initialize() {}

        static OrleansSerialization()
        {
            SerializationManager.InitializeForTesting();
        }

        internal static object Reserialize(IMessageSerializer serializer, object message)
        {
            if (serializer == null)
                return message;

            var writer = new BinaryTokenStreamWriter();
            serializer.Serialize(message, writer);

            var bytes = writer.ToByteArray();
            var reader = new BinaryTokenStreamReader(bytes);

            return serializer.Deserialize(reader);
        }
    }
}